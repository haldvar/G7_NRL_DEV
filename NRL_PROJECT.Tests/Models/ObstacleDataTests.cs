using NRL_PROJECT.Models;
using Xunit;

namespace NRL_PROJECT.Tests.Models
{
    /// <summary>
    /// Unit tests for ObstacleData model - tests default initialization behavior.
    /// </summary>
    public class ObstacleDataTests
    {
        [Fact]
        public void Default_ObstacleType_IsUkjent()
        {
            // Arrange & Act
            var obstacle = new ObstacleData();

            // Assert
            Assert.Equal("Ukjent", obstacle.ObstacleType);
        }

        [Fact]
        public void Default_MapData_IsNotNull()
        {
            // Arrange & Act
            var obstacle = new ObstacleData();

            // Assert
            Assert.NotNull(obstacle.MapData);
        }

        [Fact]
        public void Default_MapData_GeoJsonCoordinates_IsEmptyString()
        {
            // Arrange & Act
            var obstacle = new ObstacleData();

            // Assert
            Assert.NotNull(obstacle.MapData);
            Assert.Equal(string.Empty, obstacle.MapData.GeoJsonCoordinates);
        }

        [Fact]
        public void Default_ObstacleReports_IsNotNull()
        {
            // Arrange & Act
            var obstacle = new ObstacleData();

            // Assert
            Assert.NotNull(obstacle.ObstacleReports);
            Assert.Empty(obstacle.ObstacleReports);
        }
    }
}