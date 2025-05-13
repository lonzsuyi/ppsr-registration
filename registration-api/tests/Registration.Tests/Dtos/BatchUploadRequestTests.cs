using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Registration.Api.Dtos;

namespace Registration.Tests.Dtos
{
    public class BatchUploadRequestTests
    {
        [Fact]
        public void File_Should_Be_Required()
        {
            // Arrange
            var request = new BatchUploadRequest();
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, r => r.MemberNames.Contains("File"));
        }

        [Fact]
        public void File_Should_Not_Be_Null_When_Valid()
        {
            // Arrange
            var request = new BatchUploadRequest
            {
                File = new FormFile(Stream.Null, 0, 0, "Data", "test.csv")
            };
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void File_Should_Accept_Valid_CSV_File()
        {
            // Arrange
            var content = "col1,col2\nvalue1,value2";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            var request = new BatchUploadRequest
            {
                File = new FormFile(stream, 0, content.Length, "Data", "test.csv")
            };
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void File_Should_Accept_Empty_CSV_File()
        {
            // Arrange
            var content = "";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            var request = new BatchUploadRequest
            {
                File = new FormFile(stream, 0, content.Length, "Data", "test.csv")
            };
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void File_Should_Accept_Large_CSV_File()
        {
            // Arrange
            var content = new string('a', 1024 * 1024); // 1MB of data
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            var request = new BatchUploadRequest
            {
                File = new FormFile(stream, 0, content.Length, "Data", "large.csv")
            };
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }
    }
} 