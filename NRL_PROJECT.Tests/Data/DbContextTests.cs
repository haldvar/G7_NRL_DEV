using System.Linq;
using Microsoft.EntityFrameworkCore;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;
using Xunit;

namespace NRL_PROJECT.Tests.Data
{
    public class DbContextTests
    {
        private static NRL_Db_Context CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<NRL_Db_Context>()
                .UseInMemoryDatabase(databaseName: $"NRL_TestDb_{System.Guid.NewGuid()}")
                .Options;
            return new NRL_Db_Context(options);
        }

        [Fact]
        public void Can_Save_And_Load_MapData_With_Coordinates()
        {
            using var ctx = CreateInMemoryContext();

            var md = new MapData
            {
                GeometryType = "LineString",
                MapZoomLevel = 12,
                Coordinates = new System.Collections.Generic.List<MapCoordinate>
                {
                    new MapCoordinate { Latitude = 1.1, Longitude = 2.2, OrderIndex = 0 },
                    new MapCoordinate { Latitude = 3.3, Longitude = 4.4, OrderIndex = 1 }
                }
            };

            ctx.MapDatas.Add(md);
            ctx.SaveChanges();

            var loaded = ctx.MapDatas
                .Include(m => m.Coordinates)
                .FirstOrDefault(m => m.MapDataID == md.MapDataID);

            Assert.NotNull(loaded);
            Assert.Equal(2, loaded!.Coordinates.Count);
            Assert.Contains(loaded.Coordinates, c => c.Latitude == 1.1 && c.Longitude == 2.2);
        }

        [Fact]
        public void ObstacleReport_CoordinateSummary_Default_IsEmpty()
        {
            using var ctx = CreateInMemoryContext();

            var report = new ObstacleReportData
            {
                ObstacleReportComment = "Test",
                ObstacleReportDate = System.DateTime.UtcNow,
                ObstacleReportStatus = ObstacleReportData.EnumTypes.New
            };

            ctx.ObstacleReports.Add(report);
            ctx.SaveChanges();

            var loaded = ctx.ObstacleReports.FirstOrDefault(r => r.ObstacleReportID == report.ObstacleReportID);
            Assert.NotNull(loaded);
            Assert.Equal(string.Empty, loaded!.CoordinateSummary);
        }
    }
}