using System.Linq;
using NRL_PROJECT.Models;
using Xunit;

namespace NRL_PROJECT.Tests.Models
{
    public class MapDataTests
    {
        [Fact]
        public void CoordinateSummary_Point_ReturnsPointSummary()
        {
            var md = new MapData
            {
                GeometryType = "Point",
                Coordinates = new System.Collections.Generic.List<MapCoordinate>
                {
                    new MapCoordinate { Latitude = 60.3913123, Longitude = 5.3221123, OrderIndex = 0 }
                }
            };

            var summary = md.CoordinateSummary;

            Assert.NotNull(summary);
            Assert.StartsWith("Punkt (", summary);
            // Check formatting to 5 decimals
            Assert.Contains($"{md.Coordinates.First().Latitude:F5}", summary);
            Assert.Contains($"{md.Coordinates.First().Longitude:F5}", summary);
        }

        [Fact]
        public void CoordinateSummary_LineString_ReturnsLineSummary()
        {
            var md = new MapData
            {
                GeometryType = "LineString",
                Coordinates = new System.Collections.Generic.List<MapCoordinate>
                {
                    new MapCoordinate { Latitude = 60.1, Longitude = 5.1, OrderIndex = 0 },
                    new MapCoordinate { Latitude = 60.2, Longitude = 5.2, OrderIndex = 1 },
                    new MapCoordinate { Latitude = 60.3, Longitude = 5.3, OrderIndex = 2 }
                }
            };

            var summary = md.CoordinateSummary;

            Assert.NotNull(summary);
            Assert.Contains("Linje med 3 punkter", summary);
            Assert.Contains($"{md.Coordinates.First().Latitude:F5}", summary);
            Assert.Contains($"{md.Coordinates.Last().Longitude:F5}", summary);
        }

        [Fact]
        public void CoordinateSummary_NoCoordinates_ReturnsNoCoordinates()
        {
            var md = new MapData();
            var summary = md.CoordinateSummary;
            Assert.Equal("No coordinates", summary);
        }
    }
}