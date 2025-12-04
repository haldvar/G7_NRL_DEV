using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using NRL_PROJECT.Controllers;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;
using Xunit;

namespace NRL_PROJECT.Tests.Controllers
{
    public class HomeControllerTests
    {
        private static NRL_Db_Context CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<NRL_Db_Context>()
                .UseInMemoryDatabase(databaseName: $"NRL_Home_TestDb_{System.Guid.NewGuid()}")
                .Options;
            return new NRL_Db_Context(options);
        }

        private static Mock<IConfiguration> CreateMockConfiguration()
        {
            var mockConfig = new Mock<IConfiguration>();
            var mockSection = new Mock<IConfigurationSection>();
            mockSection.Setup(s => s.Value).Returns("fake-connection-string");
            
            mockConfig.Setup(c => c.GetSection("ConnectionStrings"))
                .Returns(mockSection.Object);
            mockConfig.Setup(c => c.GetSection("ConnectionStrings:DefaultConnection"))
                .Returns(mockSection.Object);
            
            return mockConfig;
        }

        [Fact]
        public async Task Index_Returns_ViewResult_When_Db_Accessible()
        {
            using var ctx = CreateInMemoryContext();
            ctx.ObstacleReports.Add(new ObstacleReportData { 
                ObstacleReportComment = "c", 
                ObstacleReportDate = System.DateTime.UtcNow, 
                ObstacleReportStatus = ObstacleReportData.EnumTypes.New 
            });
            ctx.SaveChanges();

            var mockConfig = CreateMockConfiguration();
           
            var controller = new HomeController(ctx, mockConfig.Object);
            var result = await controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            Assert.Null(view.ViewName);
            Assert.True(controller.ViewBag.ReportCount >= 0);
        }

        [Fact]
        public void About_Returns_View()
        {
            var mockConfig = CreateMockConfiguration();
            var controller = new HomeController(CreateInMemoryContext(), mockConfig.Object);
            var result = controller.About();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Privacy_Returns_View()
        {
            var mockConfig = CreateMockConfiguration();
            var controller = new HomeController(CreateInMemoryContext(), mockConfig.Object);
            var result = controller.Privacy();
            Assert.IsType<ViewResult>(result);
        }
    }
}