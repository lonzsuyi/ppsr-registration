using Registration.Application.Validation;
using Registration.Domain.ValueObjects;
using Xunit;

namespace Registration.Tests.Validation
{
    public class RegistrationValidatorTests
    {
        private readonly RegistrationRowValidator _validator = new();

        [Fact]
        public void Validate_Should_Accept_Valid_Data()
        {
            // Arrange
            var row = new Dictionary<string, string?>
            {
                ["Grantor First Name"] = "John",
                ["Grantor Middle Names"] = "James",
                ["Grantor Last Name"] = "Smith",
                ["VIN"] = "JH4DA3340GS000123",
                ["Registration start date"] = "2025-01-01",
                ["Registration duration"] = "7",
                ["SPG ACN"] = "001000004",
                ["SPG Organization Name"] = "Company A"
            };

            // Act
            var result = _validator.Validate(row);

            // Assert
            Assert.Equal("John", result.FirstName);
            Assert.Equal("James", result.MiddleNames);
            Assert.Equal("Smith", result.LastName);
            Assert.Equal("JH4DA3340GS000123", result.Vin);
            Assert.Equal(new DateTime(2025, 1, 1), result.StartDate);
            Assert.Equal(RegistrationDuration.From("7 years"), result.Duration);
            Assert.Equal("001000004", result.SpgAcn);
            Assert.Equal("Company A", result.SpgOrgName);
        }

        [Fact]
        public void Validate_Should_Accept_Empty_Middle_Names()
        {
            // Arrange
            var row = new Dictionary<string, string?>
            {
                ["Grantor First Name"] = "John",
                ["Grantor Middle Names"] = "",
                ["Grantor Last Name"] = "Smith",
                ["VIN"] = "JH4DA3340GS000123",
                ["Registration start date"] = "2025-01-01",
                ["Registration duration"] = "7",
                ["SPG ACN"] = "001000004",
                ["SPG Organization Name"] = "Company A"
            };

            // Act
            var result = _validator.Validate(row);

            // Assert
            Assert.Equal("", result.MiddleNames);
        }

        [Theory]
        [InlineData("", "Grantor First Name")]
        [InlineData(null, "Grantor First Name")]
        [InlineData("", "Grantor Last Name")]
        [InlineData(null, "Grantor Last Name")]
        [InlineData("", "VIN")]
        [InlineData(null, "VIN")]
        [InlineData("", "Registration start date")]
        [InlineData(null, "Registration start date")]
        [InlineData("", "Registration duration")]
        [InlineData(null, "Registration duration")]
        [InlineData("", "SPG ACN")]
        [InlineData(null, "SPG ACN")]
        [InlineData("", "SPG Organization Name")]
        [InlineData(null, "SPG Organization Name")]
        public void Validate_Should_Throw_For_Missing_Required_Fields(string value, string fieldName)
        {
            // Arrange
            var row = new Dictionary<string, string?>
            {
                ["Grantor First Name"] = "John",
                ["Grantor Middle Names"] = "James",
                ["Grantor Last Name"] = "Smith",
                ["VIN"] = "JH4DA3340GS000123",
                ["Registration start date"] = "2025-01-01",
                ["Registration duration"] = "7",
                ["SPG ACN"] = "001000004",
                ["SPG Organization Name"] = "Company A"
            };
            row[fieldName] = value;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _validator.Validate(row));
        }

        [Theory]
        [InlineData("123", "VIN must be 17 characters")]
        [InlineData("123456789012345678", "VIN must be 17 characters")]
        [InlineData("1234567890123456A", "VIN must be 17 characters")]
        public void Validate_Should_Throw_For_Invalid_VIN(string vin, string expectedError)
        {
            // Arrange
            var row = new Dictionary<string, string?>
            {
                ["Grantor First Name"] = "John",
                ["Grantor Middle Names"] = "James",
                ["Grantor Last Name"] = "Smith",
                ["VIN"] = vin,
                ["Registration start date"] = "2025-01-01",
                ["Registration duration"] = "7",
                ["SPG ACN"] = "001000004",
                ["SPG Organization Name"] = "Company A"
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _validator.Validate(row));
            Assert.Contains(expectedError, exception.Message);
        }

        [Theory]
        [InlineData("invalid", "Invalid date")]
        [InlineData("2025-13-01", "Invalid date")]
        [InlineData("2025-00-01", "Invalid date")]
        public void Validate_Should_Throw_For_Invalid_Date(string date, string expectedError)
        {
            // Arrange
            var row = new Dictionary<string, string?>
            {
                ["Grantor First Name"] = "John",
                ["Grantor Middle Names"] = "James",
                ["Grantor Last Name"] = "Smith",
                ["VIN"] = "JH4DA3340GS000123",
                ["Registration start date"] = date,
                ["Registration duration"] = "7",
                ["SPG ACN"] = "001000004",
                ["SPG Organization Name"] = "Company A"
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _validator.Validate(row));
            Assert.Contains(expectedError, exception.Message);
        }

        [Theory]
        [InlineData("6", "Invalid duration")]
        [InlineData("8", "Invalid duration")]
        [InlineData("24", "Invalid duration")]
        [InlineData("26", "Invalid duration")]
        [InlineData("invalid", "Invalid duration")]
        public void Validate_Should_Throw_For_Invalid_Duration(string duration, string expectedError)
        {
            // Arrange
            var row = new Dictionary<string, string?>
            {
                ["Grantor First Name"] = "John",
                ["Grantor Middle Names"] = "James",
                ["Grantor Last Name"] = "Smith",
                ["VIN"] = "JH4DA3340GS000123",
                ["Registration start date"] = "2025-01-01",
                ["Registration duration"] = duration,
                ["SPG ACN"] = "001000004",
                ["SPG Organization Name"] = "Company A"
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _validator.Validate(row));
            Assert.Contains(expectedError, exception.Message);
        }

        [Theory]
        [InlineData("12345678", "SPG ACN must be 9 digits")]
        [InlineData("1234567890", "SPG ACN must be 9 digits")]
        [InlineData("12345678A", "SPG ACN must be 9 digits")]
        public void Validate_Should_Throw_For_Invalid_ACN(string acn, string expectedError)
        {
            // Arrange
            var row = new Dictionary<string, string?>
            {
                ["Grantor First Name"] = "John",
                ["Grantor Middle Names"] = "James",
                ["Grantor Last Name"] = "Smith",
                ["VIN"] = "JH4DA3340GS000123",
                ["Registration start date"] = "2025-01-01",
                ["Registration duration"] = "7",
                ["SPG ACN"] = acn,
                ["SPG Organization Name"] = "Company A"
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _validator.Validate(row));
            Assert.Contains(expectedError, exception.Message);
        }
    }
} 