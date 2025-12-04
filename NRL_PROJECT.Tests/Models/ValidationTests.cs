using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NRL_PROJECT.Models;
using Xunit;

namespace NRL_PROJECT.Tests.Models
{
    /// <summary>
    /// Unit tests for model validation attributes
    /// </summary>
    public class ValidationTests
    {
        [Fact]
        public void ObstacleData_ObstacleType_IsRequired()
        {
            // Arrange
            var obstacle = new ObstacleData { ObstacleType = null };
            
            // Act
            var results = ValidateModel(obstacle);
            
            // Assert
            Assert.Contains(results, r => r.MemberNames.Contains("ObstacleType"));
        }

        [Fact]
        public void ObstacleData_ObstacleImageURL_MaxLength255()
        {
            // Arrange
            var obstacle = new ObstacleData 
            { 
                ObstacleImageURL = new string('a', 300) // 300 characters
            };
            
            // Act
            var results = ValidateModel(obstacle);
            
            // Assert
            Assert.Contains(results, r => r.MemberNames.Contains("ObstacleImageURL"));
        }

        private List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model);
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            return validationResults;
        }
    }
}