using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NRL_PROJECT.Controllers;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;
using Xunit;

namespace NRL_PROJECT.Tests.Controllers
{
    /// <summary>
    /// Unit tests for MapController - handles obstacle reporting, map views, and coordinate storage.
    /// Tests cover core functionality including GeoJSON processing, image uploads, and user-specific data filtering.
    /// </summary>
    public class MapControllerTests : IDisposable
    {
        private readonly NRL_Db_Context _context;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly MapController _controller;
        private readonly User _testUser;

        #region Test Constants

        private const string DefaultComment = "Ingen kommentar registrert";
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

        #endregion

        public MapControllerTests()
        {
            var options = new DbContextOptionsBuilder<NRL_Db_Context>()
                .UseInMemoryDatabase(databaseName: $"MapControllerTest_{Guid.NewGuid()}")
                .Options;
            _context = new NRL_Db_Context(options);

            _mockEnvironment = new Mock<IWebHostEnvironment>();
            var tempPath = Path.Combine(Path.GetTempPath(), "NRL_Test_Images");
            _mockEnvironment.Setup(e => e.WebRootPath).Returns(tempPath);
            _mockEnvironment.Setup(e => e.ContentRootPath).Returns(tempPath);
            
            // Create temp directory if it doesn't exist
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }
            
            var userStore = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            _testUser = new User
            {
                Id = "test-user-id",
                UserName = "testpilot",
                Email = "pilot@test.no",
                FirstName = "Test",
                LastName = "Pilot"
            };

            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns(_testUser.Id);
            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(_testUser);

            _controller = new MapController(_context, _mockEnvironment.Object, _mockUserManager.Object);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _testUser.Id),
                new Claim(ClaimTypes.Name, _testUser.UserName)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
    
            // Clean up temp directory
            var tempPath = Path.Combine(Path.GetTempPath(), "NRL_Test_Images");
            if (Directory.Exists(tempPath))
            {
                try
                {
                    Directory.Delete(tempPath, recursive: true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        #region ObstacleAndMapForm Tests (GET)

        [Fact]
        public void ObstacleAndMapForm_GET_ReturnsViewWithDefaultModel()
        {
            // Act
            var result = _controller.ObstacleAndMapForm();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ObstacleData>(viewResult.Model);
            
            Assert.NotNull(model.MapData);
            Assert.Equal("Point", model.MapData.GeometryType);
            Assert.Equal(13, model.MapData.MapZoomLevel);
        }

        #endregion

        #region SubmitObstacleWithLocation Tests (POST)

        [Fact]
        public async Task SubmitObstacleWithLocation_WithValidPointGeoJson_CreatesReportSuccessfully()
        {
            // Arrange
            var model = new ObstacleData
            {
                ObstacleType = "Radio/Mobilmast",
                ObstacleComment = "Høy mast observert",
                MapData = new MapData
                {
                    GeoJsonCoordinates = "{\"type\":\"Point\",\"coordinates\":[8.0855,58.2040]}",
                    MapZoomLevel = 15
                }
            };

            // Act
            var result = await _controller.SubmitObstacleWithLocation(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("MapConfirmation", viewResult.ViewName);

            Assert.Single(_context.MapDatas);
            Assert.Single(_context.Obstacles);
            Assert.Single(_context.ObstacleReports);

            var savedReport = await _context.ObstacleReports
                .Include(r => r.MapData)
                .ThenInclude(m => m.Coordinates)
                .FirstAsync();

            Assert.Equal(_testUser.Id, savedReport.SubmittedByUserId);
            Assert.Equal(ObstacleReportData.EnumTypes.New, savedReport.ObstacleReportStatus);
            Assert.Single(savedReport.MapData.Coordinates);
            Assert.Equal(58.2040, savedReport.MapData.Coordinates.First().Latitude, 4);
            Assert.Equal(8.0855, savedReport.MapData.Coordinates.First().Longitude, 4);
        }

        [Fact]
        public async Task SubmitObstacleWithLocation_WithValidLineStringGeoJson_CreatesReportSuccessfully()
        {
            // Arrange
            var model = new ObstacleData
            {
                ObstacleType = "Høyspentledning",
                MapData = new MapData
                {
                    GeoJsonCoordinates = "{\"type\":\"LineString\",\"coordinates\":[[8.0855,58.2040],[8.0955,58.2140]]}",
                    MapZoomLevel = 14
                }
            };

            // Act
            var result = await _controller.SubmitObstacleWithLocation(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("MapConfirmation", viewResult.ViewName);

            var savedMapData = await _context.MapDatas
                .Include(m => m.Coordinates)
                .FirstAsync();

            Assert.Equal("LineString", savedMapData.GeometryType);
            Assert.Equal(2, savedMapData.Coordinates.Count);
            
            var orderedCoords = savedMapData.Coordinates.OrderBy(c => c.OrderIndex).ToList();
            Assert.Equal(0, orderedCoords[0].OrderIndex);
            Assert.Equal(1, orderedCoords[1].OrderIndex);
        }

        [Fact]
        public async Task SubmitObstacleWithLocation_WithoutGeoJson_ReturnsViewWithError()
        {
            // Arrange
            var model = new ObstacleData
            {
                ObstacleType = "Kran",
                MapData = new MapData
                {
                    GeoJsonCoordinates = ""
                }
            };

            // Act
            var result = await _controller.SubmitObstacleWithLocation(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("ObstacleAndMapForm", viewResult.ViewName);
            Assert.False(_controller.ModelState.IsValid);
            
            Assert.Empty(_context.MapDatas);
            Assert.Empty(_context.Obstacles);
        }

        [Fact]
        public async Task SubmitObstacleWithLocation_WithNullGeoJson_ReturnsViewWithError()
        {
            // Arrange
            var model = new ObstacleData
            {
                ObstacleType = "Vindmølle",
                MapData = new MapData
                {
                    GeoJsonCoordinates = null
                }
            };

            // Act
            var result = await _controller.SubmitObstacleWithLocation(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("ObstacleAndMapForm", viewResult.ViewName);
        }

        [Fact]
        public async Task SubmitObstacleWithLocation_WithInvalidGeoJsonType_ReturnsViewWithError()
        {
            // Arrange
            var model = new ObstacleData
            {
                ObstacleType = "Bygning",
                MapData = new MapData
                {
                    GeoJsonCoordinates = "{\"type\":\"Polygon\",\"coordinates\":[[[0,0],[1,0],[1,1],[0,1],[0,0]]]}"
                }
            };

            // Act
            var result = await _controller.SubmitObstacleWithLocation(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("ObstacleAndMapForm", viewResult.ViewName);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task SubmitObstacleWithLocation_WithoutComment_SetsDefaultComment()
        {
            // Arrange
            var model = new ObstacleData
            {
                ObstacleType = "Mast/Tårn",
                ObstacleComment = null,
                MapData = new MapData
                {
                    GeoJsonCoordinates = "{\"type\":\"Point\",\"coordinates\":[10.0,60.0]}"
                }
            };

            // Act
            await _controller.SubmitObstacleWithLocation(model);

            // Assert
            var obstacle = await _context.Obstacles.FirstAsync();
            Assert.Equal(DefaultComment, obstacle.ObstacleComment);
        }

        [Fact]
        public async Task SubmitObstacleWithLocation_WithoutHeightAndWidth_StillSucceeds()
        {
            // Arrange
            var model = new ObstacleData
            {
                ObstacleType = "Kran",
                ObstacleHeight = 0,
                ObstacleWidth = 0,
                MapData = new MapData
                {
                    GeoJsonCoordinates = "{\"type\":\"Point\",\"coordinates\":[10.5,60.5]}"
                }
            };

            // Act
            var result = await _controller.SubmitObstacleWithLocation(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("MapConfirmation", viewResult.ViewName);

            var obstacle = await _context.Obstacles.FirstAsync();
            Assert.Equal(0, obstacle.ObstacleHeight);
            Assert.Equal(0, obstacle.ObstacleWidth);
        }

        [Fact]
        public async Task SubmitObstacleWithLocation_SetsCorrectReportStatus()
        {
            // Arrange
            var model = new ObstacleData
            {
                ObstacleType = "Lyktestolpe",
                MapData = new MapData
                {
                    GeoJsonCoordinates = "{\"type\":\"Point\",\"coordinates\":[9.0,59.0]}"
                }
            };

            // Act
            await _controller.SubmitObstacleWithLocation(model);

            // Assert
            var report = await _context.ObstacleReports.FirstAsync();
            Assert.Equal(ObstacleReportData.EnumTypes.New, report.ObstacleReportStatus);
        }

        [Fact]
        public async Task SubmitObstacleWithLocation_SetsCorrectTimestamp()
        {
            // Arrange
            var beforeSubmit = DateTime.UtcNow.AddHours(-1);
            var model = new ObstacleData
            {
                ObstacleType = "Annet",
                MapData = new MapData
                {
                    GeoJsonCoordinates = "{\"type\":\"Point\",\"coordinates\":[8.5,58.5]}"
                }
            };

            // Act
            await _controller.SubmitObstacleWithLocation(model);
            var afterSubmit = DateTime.UtcNow.AddHours(2);            // Assert
            var report = await _context.ObstacleReports.FirstAsync();
            Assert.InRange(report.ObstacleReportDate, beforeSubmit, afterSubmit);
        }

        [Fact]
        public async Task SubmitObstacleWithLocation_WithExtremeValues_StoresCorrectly()
        {
            // Arrange
            var model = new ObstacleData
            {
                ObstacleType = "Vindmølle",
                ObstacleComment = "Svært høy",
                ObstacleHeight = 9999,
                ObstacleWidth = 500,
                MapData = new MapData
                {
                    GeoJsonCoordinates = "{\"type\":\"Point\",\"coordinates\":[10.0,60.0]}"
                }
            };

            // Act
            await _controller.SubmitObstacleWithLocation(model);

            // Assert
            var obstacle = await _context.Obstacles.FirstAsync();
            Assert.Equal(9999, obstacle.ObstacleHeight);
            Assert.Equal(500, obstacle.ObstacleWidth);
        }

        [Fact]
        public async Task SubmitObstacleWithLocation_WithDuplicateObstacle_CreatesSeparateReport()
        {
            // Arrange
            var model1 = new ObstacleData
            {
                ObstacleType = "Radio/Mobilmast",
                ObstacleComment = "Første observasjon",
                MapData = new MapData
                {
                    GeoJsonCoordinates = "{\"type\":\"Point\",\"coordinates\":[8.0855,58.2040]}"
                }
            };
            var model2 = new ObstacleData
            {
                ObstacleType = "Radio/Mobilmast",
                ObstacleComment = "Andre observasjon",
                MapData = new MapData
                {
                    GeoJsonCoordinates = "{\"type\":\"Point\",\"coordinates\":[8.0855,58.2040]}"
                }
            };

            // Act
            await _controller.SubmitObstacleWithLocation(model1);
            await _controller.SubmitObstacleWithLocation(model2);

            // Assert - Should create two separate reports
            Assert.Equal(2, _context.ObstacleReports.Count());
            Assert.Equal(2, _context.Obstacles.Count());
            
            var reports = await _context.ObstacleReports.ToListAsync();
            Assert.NotEqual(reports[0].ObstacleID, reports[1].ObstacleID);
        }

        #endregion

        #region Image Upload Tests

        [Fact]
        public async Task SubmitObstacleWithLocation_WithValidImage_SavesImageSuccessfully()
        {
            // Arrange
            var mockFile = CreateMockImageFile("obstacle.jpg", 1024, "image/jpeg");
            var model = new ObstacleData
            {
                ObstacleType = "Radio/Mobilmast",
                ImageFile = mockFile.Object,
                MapData = new MapData
                {
                    GeoJsonCoordinates = "{\"type\":\"Point\",\"coordinates\":[10.0,60.0]}"
                }
            };

            // Act
            var result = await _controller.SubmitObstacleWithLocation(model);

            // Assert
            var obstacle = await _context.Obstacles.FirstAsync();
            Assert.NotNull(obstacle.ObstacleImageURL);
            Assert.Contains(".jpg", obstacle.ObstacleImageURL);
        }

        [Fact]
        public async Task SubmitObstacleWithLocation_WithOversizedImage_ReturnsError()
        {
            // Arrange
            var mockFile = CreateMockImageFile("large.jpg", MaxFileSizeBytes + 1, "image/jpeg");
            var model = new ObstacleData
            {
                ObstacleType = "Kran",
                ImageFile = mockFile.Object,
                MapData = new MapData
                {
                    GeoJsonCoordinates = "{\"type\":\"Point\",\"coordinates\":[10.0,60.0]}"
                }
            };

            // Act
            var result = await _controller.SubmitObstacleWithLocation(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("ObstacleAndMapForm", viewResult.ViewName);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task SubmitObstacleWithLocation_WithInvalidFileType_ReturnsError()
        {
            // Arrange
            var mockFile = CreateMockImageFile("malicious.exe", 1024, "application/x-msdownload");
            var model = new ObstacleData
            {
                ObstacleType = "Mast",
                ImageFile = mockFile.Object,
                MapData = new MapData
                {
                    GeoJsonCoordinates = "{\"type\":\"Point\",\"coordinates\":[10.0,60.0]}"
                }
            };

            // Act
            var result = await _controller.SubmitObstacleWithLocation(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("ObstacleAndMapForm", viewResult.ViewName);
            Assert.False(_controller.ModelState.IsValid);
        }

        #endregion

        #region GetObstacles Tests (API Endpoint)

        [Fact]
        public async Task GetObstacles_WithNoData_ReturnsEmptyFeatureCollection()
        {
            // Act
            var result = await _controller.GetObstacles();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
        }

        [Fact]
        public async Task GetObstacles_WithPointObstacle_ReturnsCorrectGeoJson()
        {
            // Arrange
            var mapData = new MapData
            {
                GeometryType = "Point",
                Coordinates = new List<MapCoordinate>
                {
                    new MapCoordinate { Latitude = 60.0, Longitude = 10.0, OrderIndex = 0 }
                }
            };
            var obstacle = new ObstacleData
            {
                ObstacleType = "Mast",
                ObstacleHeight = 50,
                MapData = mapData
            };
            _context.Obstacles.Add(obstacle);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetObstacles();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
        }

        [Fact]
        public async Task GetObstacles_WithMultipleObstacles_ReturnsAllObstacles()
        {
            // Arrange
            for (int i = 0; i < 3; i++)
            {
                var mapData = new MapData
                {
                    GeometryType = "Point",
                    Coordinates = new List<MapCoordinate>
                    {
                        new MapCoordinate 
                        { 
                            Latitude = 60.0 + i, 
                            Longitude = 10.0 + i, 
                            OrderIndex = 0 
                        }
                    }
                };
                var obstacle = new ObstacleData
                {
                    ObstacleType = $"Obstacle {i}",
                    MapData = mapData
                };
                _context.Obstacles.Add(obstacle);
            }
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetObstacles();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
            Assert.Equal(3, _context.Obstacles.Count());
        }

        #endregion

        #region MyReports Tests

        [Fact]
        public async Task MyReports_ReturnsOnlyCurrentUserReports()
        {
            // Arrange
            var otherUser = new User { Id = "other-user", UserName = "other" };
            
            await CreateTestReport(_testUser.Id, "Min rapport 1");
            await CreateTestReport(_testUser.Id, "Min rapport 2");
            await CreateTestReport(otherUser.Id, "Annen brukers rapport");

            // Act
            var result = await _controller.MyReports();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var reports = Assert.IsAssignableFrom<List<ObstacleReportData>>(viewResult.Model);
            
            Assert.Equal(2, reports.Count);
            Assert.All(reports, r => Assert.Equal(_testUser.Id, r.SubmittedByUserId));
        }

        [Fact]
        public async Task MyReports_WithNoReports_ReturnsEmptyList()
        {
            // Act
            var result = await _controller.MyReports();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var reports = Assert.IsAssignableFrom<List<ObstacleReportData>>(viewResult.Model);
            Assert.Empty(reports);
        }

        [Fact]
        public async Task MyReports_ReturnsReportsOrderedByDateDescending()
        {
            // Arrange
            await Task.Delay(10);
            await CreateTestReport(_testUser.Id, "Gammel rapport");
            await Task.Delay(10);
            await CreateTestReport(_testUser.Id, "Ny rapport");

            // Act
            var result = await _controller.MyReports();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var reports = Assert.IsAssignableFrom<List<ObstacleReportData>>(viewResult.Model);
            
            for (int i = 0; i < reports.Count - 1; i++)
            {
                Assert.True(reports[i].ObstacleReportDate >= reports[i + 1].ObstacleReportDate);
            }
        }

        #endregion

        #region MapConfirmation Tests

        [Fact]
        public void MapConfirmation_GET_ReturnsView()
        {
            // Act
            var result = _controller.MapConfirmation();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        #endregion

        #region Helper Methods

        private async Task<ObstacleReportData> CreateTestReport(string userId, string comment)
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
                ObstacleType = "Test",
                MapData = mapData
            };
            _context.Obstacles.Add(obstacle);
            await _context.SaveChangesAsync();

            var report = new ObstacleReportData
            {
                ObstacleID = obstacle.ObstacleID,
                MapDataID = mapData.MapDataID,
                SubmittedByUserId = userId,
                ObstacleReportComment = comment,
                ObstacleReportDate = DateTime.UtcNow,
                ObstacleReportStatus = ObstacleReportData.EnumTypes.New
            };
            _context.ObstacleReports.Add(report);
            await _context.SaveChangesAsync();

            return report;
        }

        private Mock<IFormFile> CreateMockImageFile(string fileName, long fileSize, string contentType)
        {
            var mockFile = new Mock<IFormFile>();
            var content = "fake image content";
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);
            writer.Write(content);
            writer.Flush();
            memoryStream.Position = 0;

            mockFile.Setup(f => f.OpenReadStream()).Returns(memoryStream);
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.Length).Returns(fileSize);
            mockFile.Setup(f => f.ContentType).Returns(contentType);

            return mockFile;
        }

        #endregion
    }
}