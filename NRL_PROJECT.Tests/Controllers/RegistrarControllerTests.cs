using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NRL_PROJECT.Controllers;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;
using NRL_PROJECT.Models.ViewModels;
using Xunit;

namespace NRL_PROJECT.Tests.Controllers
{
    /// <summary>
    /// Tests for RegistrarController - handles report processing, status updates,
    /// obstacle data modifications, and registrar assignments.
    /// </summary>
    public class RegistrarControllerTests : IDisposable
    {
        private readonly NRL_Db_Context _context;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly RegistrarController _controller;
        private readonly User _registrarUser;
        private readonly User _pilotUser;

        public RegistrarControllerTests()
        {
            // Create in-memory database
            var options = new DbContextOptionsBuilder<NRL_Db_Context>()
                .UseInMemoryDatabase(databaseName: $"RegistrarTest_{Guid.NewGuid()}")
                .Options;
            _context = new NRL_Db_Context(options);

            // Setup mock UserManager
            var userStore = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            // Create test users
            _registrarUser = new User
            {
                Id = "registrar-id",
                UserName = "registrar1",
                Email = "reg@test.no",
                FirstName = "Reg",
                LastName = "Istrar"
            };

            _pilotUser = new User
            {
                Id = "pilot-id",
                UserName = "pilot1",
                Email = "pilot@test.no",
                FirstName = "Test",
                LastName = "Pilot"
            };

            // Add users to context
            _context.Users.AddRange(_registrarUser, _pilotUser);
            _context.SaveChanges();

            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(_registrarUser.Id);
            _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string id) => _context.Users.FirstOrDefault(u => u.Id == id));
            _mockUserManager.Setup(m => m.IsInRoleAsync(It.IsAny<User>(), "Registrar"))
                .ReturnsAsync(true);
            _mockUserManager.Setup(m => m.GetUsersInRoleAsync("Registrar"))
                .ReturnsAsync(new List<User> { _registrarUser });

            // Create controller
            _controller = new RegistrarController(_context, _mockUserManager.Object);

            // Setup HttpContext
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _registrarUser.Id),
                new Claim(ClaimTypes.Role, "Registrar")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Setup TempData
            _controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region UpdateStatus Tests - Critical for workflow

        [Fact]
        public async Task UpdateStatus_ToUnderBehandling_UpdatesStatusCorrectly()
        {
            // Arrange
            var report = await CreateTestReport(ObstacleReportData.EnumTypes.New);

            // Act
            var result = await _controller.UpdateStatus(report.ObstacleReportID, "UnderBehandling", null);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ReportDetails", redirectResult.ActionName);

            var updatedReport = await _context.ObstacleReports.FindAsync(report.ObstacleReportID);
            Assert.Equal(ObstacleReportData.EnumTypes.InProgress, updatedReport.ObstacleReportStatus);
        }

        [Fact]
        public async Task UpdateStatus_ToGodkjent_UpdatesStatusCorrectly()
        {
            // Arrange
            var report = await CreateTestReport(ObstacleReportData.EnumTypes.InProgress);

            // Act
            var result = await _controller.UpdateStatus(report.ObstacleReportID, "Godkjent", null);

            // Assert
            var updatedReport = await _context.ObstacleReports.FindAsync(report.ObstacleReportID);
            Assert.Equal(ObstacleReportData.EnumTypes.Resolved, updatedReport.ObstacleReportStatus);
        }

        [Fact]
        public async Task UpdateStatus_ToAvvist_UpdatesStatusCorrectly()
        {
            // Arrange
            var report = await CreateTestReport(ObstacleReportData.EnumTypes.New);

            // Act
            var result = await _controller.UpdateStatus(report.ObstacleReportID, "Avvist", null);

            // Assert
            var updatedReport = await _context.ObstacleReports.FindAsync(report.ObstacleReportID);
            Assert.Equal(ObstacleReportData.EnumTypes.Deleted, updatedReport.ObstacleReportStatus);
        }

        [Fact]
        public async Task UpdateStatus_WithComment_SavesComment()
        {
            // Arrange
            var report = await CreateTestReport(ObstacleReportData.EnumTypes.New);
            var comment = "Behandlet og godkjent av registerfører";

            // Act
            await _controller.UpdateStatus(report.ObstacleReportID, "Godkjent", comment);

            // Assert
            var updatedReport = await _context.ObstacleReports.FindAsync(report.ObstacleReportID);
            Assert.Equal(comment, updatedReport.ObstacleReportComment);
        }

        [Fact]
        public async Task UpdateStatus_WithNonExistentReport_ReturnsNotFound()
        {
            // Act
            var result = await _controller.UpdateStatus(99999, "Godkjent", null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateStatus_SetsTempDataMessage()
        {
            // Arrange
            var report = await CreateTestReport(ObstacleReportData.EnumTypes.New);

            // Act
            await _controller.UpdateStatus(report.ObstacleReportID, "UnderBehandling", null);

            // Assert
            Assert.True(_controller.TempData.ContainsKey("StatusChanged"));
        }

        #endregion

        #region UpdateObstacleData Tests - Registrar adds height/type

        [Fact]
        public async Task UpdateObstacleData_UpdatesHeightCorrectly()
        {
            // Arrange - Pilot submitted without height, registrar adds it
            var report = await CreateTestReport(ObstacleReportData.EnumTypes.New);
            var newHeight = 125.5;

            // Act
            var result = await _controller.UpdateObstacleData(
                report.ObstacleReportID, 
                null,  // Keep existing type
                newHeight);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            
            var updatedReport = await _context.ObstacleReports
                .Include(r => r.Obstacle)
                .FirstAsync(r => r.ObstacleReportID == report.ObstacleReportID);
            
            Assert.Equal(newHeight, updatedReport.Obstacle.ObstacleHeight);
        }

        [Fact]
        public async Task UpdateObstacleData_UpdatesTypeCorrectly()
        {
            // Arrange
            var report = await CreateTestReport(ObstacleReportData.EnumTypes.New);
            var newType = "Vindmølle";

            // Act
            await _controller.UpdateObstacleData(report.ObstacleReportID, newType, null);

            // Assert
            var updatedReport = await _context.ObstacleReports
                .Include(r => r.Obstacle)
                .FirstAsync(r => r.ObstacleReportID == report.ObstacleReportID);
            
            Assert.Equal(newType, updatedReport.Obstacle.ObstacleType);
        }

        [Fact]
        public async Task UpdateObstacleData_WithInvalidHeight_ReturnsError()
        {
            // Arrange
            var report = await CreateTestReport(ObstacleReportData.EnumTypes.New);

            // Act - Height over 10000 should fail
            var result = await _controller.UpdateObstacleData(
                report.ObstacleReportID, 
                null, 
                15000);  // Invalid height

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.True(_controller.TempData.ContainsKey("Error"));
        }

        [Fact]
        public async Task UpdateObstacleData_WithNegativeHeight_ReturnsError()
        {
            // Arrange
            var report = await CreateTestReport(ObstacleReportData.EnumTypes.New);

            // Act
            var result = await _controller.UpdateObstacleData(
                report.ObstacleReportID, 
                null, 
                -50);  // Negative height invalid

            // Assert
            Assert.True(_controller.TempData.ContainsKey("Error"));
        }

        [Fact]
        public async Task UpdateObstacleData_WithNonExistentReport_RedirectsWithError()
        {
            // Act
            var result = await _controller.UpdateObstacleData(99999, "Type", 100);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("RegistrarView", redirectResult.ActionName);
            Assert.True(_controller.TempData.ContainsKey("Error"));
        }

        #endregion

        #region TransferReport Tests - Delegate to another registrar

        [Fact]
        public async Task TransferReport_AssignsNewRegistrar()
        {
            // Arrange
            var report = await CreateTestReport(ObstacleReportData.EnumTypes.InProgress);
            var newRegistrar = new User
            {
                Id = "new-registrar-id",
                UserName = "registrar2",
                FirstName = "New",
                LastName = "Registrar"
            };
            _context.Users.Add(newRegistrar);
            await _context.SaveChangesAsync();

            _mockUserManager.Setup(m => m.FindByIdAsync("new-registrar-id"))
                .ReturnsAsync(newRegistrar);

            // Act
            var result = await _controller.TransferReport(
                report.ObstacleReportID, 
                newRegistrar.Id);

            // Assert
            var updatedReport = await _context.ObstacleReports.FindAsync(report.ObstacleReportID);
            Assert.Equal(newRegistrar.Id, updatedReport.ReviewedByUserID);
        }

        [Fact]
        public async Task TransferReport_WithEmptyUserId_ReturnsError()
        {
            // Arrange
            var report = await CreateTestReport(ObstacleReportData.EnumTypes.New);

            // Act
            var result = await _controller.TransferReport(report.ObstacleReportID, "");

            // Assert
            Assert.True(_controller.TempData.ContainsKey("Error"));
        }

        #endregion

        #region SaveComment Tests

        [Fact]
        public async Task SaveComment_UpdatesCommentCorrectly()
        {
            // Arrange
            var report = await CreateTestReport(ObstacleReportData.EnumTypes.New);
            var newComment = "Registerfører har lagt til kommentar";

            // Act
            var result = await _controller.SaveComment(report.ObstacleReportID, newComment);

            // Assert
            var updatedReport = await _context.ObstacleReports.FindAsync(report.ObstacleReportID);
            Assert.Equal(newComment, updatedReport.ObstacleReportComment);
        }

        [Fact]
        public async Task SaveComment_WithNullComment_SetsEmptyString()
        {
            // Arrange
            var report = await CreateTestReport(ObstacleReportData.EnumTypes.New);

            // Act
            await _controller.SaveComment(report.ObstacleReportID, null);

            // Assert
            var updatedReport = await _context.ObstacleReports.FindAsync(report.ObstacleReportID);
            Assert.Equal(string.Empty, updatedReport.ObstacleReportComment);
        }

        [Fact]
        public async Task SaveComment_TrimsWhitespace()
        {
            // Arrange
            var report = await CreateTestReport(ObstacleReportData.EnumTypes.New);

            // Act
            await _controller.SaveComment(report.ObstacleReportID, "  Trimmet kommentar  ");

            // Assert
            var updatedReport = await _context.ObstacleReports.FindAsync(report.ObstacleReportID);
            Assert.Equal("Trimmet kommentar", updatedReport.ObstacleReportComment);
        }

        #endregion

        #region RegistrarView Tests (Overview)

        [Fact]
        public async Task RegistrarView_ReturnsAllReports()
        {
            // Arrange
            await CreateTestReport(ObstacleReportData.EnumTypes.New);
            await CreateTestReport(ObstacleReportData.EnumTypes.InProgress);
            await CreateTestReport(ObstacleReportData.EnumTypes.Resolved);

            // Act
            var result = await _controller.RegistrarView();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<ObstacleReportViewModel>>(viewResult.Model);
            Assert.Equal(3, model.Count);
        }

        [Fact]
        public async Task RegistrarView_WithStatusFilter_FiltersCorrectly()
        {
            // Arrange
            await CreateTestReport(ObstacleReportData.EnumTypes.New);
            await CreateTestReport(ObstacleReportData.EnumTypes.New);
            await CreateTestReport(ObstacleReportData.EnumTypes.Resolved);

            // Act
            var result = await _controller.RegistrarView(status: "Ny");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<ObstacleReportViewModel>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task RegistrarView_WithSearchQuery_FiltersCorrectly()
        {
            // Arrange - Create reports with specific types
            var report1 = await CreateTestReportWithType("Vindmølle");
            var report2 = await CreateTestReportWithType("Kran");
            var report3 = await CreateTestReportWithType("Vindmølle");

            // Act
            var result = await _controller.RegistrarView(q: "Vindmølle");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<ObstacleReportViewModel>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }

        #endregion

        #region ReportDetails Tests

        [Fact]
        public async Task ReportDetails_WithValidId_ReturnsCorrectReport()
        {
            // Arrange
            var report = await CreateTestReport(ObstacleReportData.EnumTypes.New);

            // Act
            var result = await _controller.ReportDetails(report.ObstacleReportID);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ObstacleReportViewModel>(viewResult.Model);
            Assert.Equal(report.ObstacleReportID, model.ObstacleReportID);
        }

        [Fact]
        public async Task ReportDetails_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.ReportDetails(99999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ReportDetails_IncludesRegistrarDropdown()
        {
            // Arrange
            var report = await CreateTestReport(ObstacleReportData.EnumTypes.New);

            // Act
            var result = await _controller.ReportDetails(report.ObstacleReportID);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.ViewData["Registrars"]);
        }

        #endregion

        #region Helper Methods

        private async Task<ObstacleReportData> CreateTestReport(ObstacleReportData.EnumTypes status)
        {
            var mapData = new MapData
            {
                GeometryType = "Point",
                GeoJsonCoordinates = "{\"type\":\"Point\",\"coordinates\":[10,60]}",
                Coordinates = new List<MapCoordinate>
                {
                    new MapCoordinate { Latitude = 60.0, Longitude = 10.0, OrderIndex = 0 }
                }
            };
            _context.MapDatas.Add(mapData);
            await _context.SaveChangesAsync();

            var obstacle = new ObstacleData
            {
                ObstacleType = "Ukjent",
                ObstacleHeight = 0,
                MapData = mapData
            };
            _context.Obstacles.Add(obstacle);
            await _context.SaveChangesAsync();

            var report = new ObstacleReportData
            {
                ObstacleID = obstacle.ObstacleID,
                Obstacle = obstacle,
                MapDataID = mapData.MapDataID,
                SubmittedByUserId = _pilotUser.Id,
                ObstacleReportComment = "Testrapport",
                ObstacleReportDate = DateTime.UtcNow,
                ObstacleReportStatus = status
            };
            _context.ObstacleReports.Add(report);
            await _context.SaveChangesAsync();

            return report;
        }

        private async Task<ObstacleReportData> CreateTestReportWithType(string obstacleType)
        {
            var mapData = new MapData
            {
                GeometryType = "Point",
                Coordinates = new List<MapCoordinate>
                {
                    new MapCoordinate { Latitude = 60.0, Longitude = 10.0, OrderIndex = 0 }
                }
            };
            _context.MapDatas.Add(mapData);

            var obstacle = new ObstacleData
            {
                ObstacleType = obstacleType,
                MapData = mapData
            };
            _context.Obstacles.Add(obstacle);

            var report = new ObstacleReportData
            {
                Obstacle = obstacle,
                MapData = mapData,
                SubmittedByUserId = _pilotUser.Id,
                ObstacleReportDate = DateTime.UtcNow,
                ObstacleReportStatus = ObstacleReportData.EnumTypes.New
            };
            _context.ObstacleReports.Add(report);
            await _context.SaveChangesAsync();

            return report;
        }

        #endregion
    }
}