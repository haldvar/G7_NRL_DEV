using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using NRL_PROJECT.Models;
using Xunit;

namespace NRL_PROJECT.Tests.Models
{
    public class ObstacleDataTests
    {
        [Fact]
        public void Default_MapData_GeoJsonCoordinates_IsEmptyString()
        {
            var o = new ObstacleData();
            Assert.NotNull(o.MapData);
            Assert.Equal(string.Empty, o.MapData.GeoJsonCoordinates);
        }

        [Fact]
        public void ImageFile_Property_Has_NotMappedAttribute()
        {
            var prop = typeof(ObstacleData).GetProperty(nameof(ObstacleData.ImageFile));
            Assert.NotNull(prop);
            var notMapped = prop.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute), inherit: true);
            Assert.NotNull(notMapped);
            Assert.True(notMapped.Any());
        }

        [Fact]
        public void ObstacleReports_Collection_IsInitialized()
        {
            var o = new ObstacleData();
            Assert.NotNull(o.ObstacleReports);
        }
    }
}