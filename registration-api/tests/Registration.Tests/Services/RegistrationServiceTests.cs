namespace Registration.Tests.Services
{
    using Xunit;
    using Moq;
    using FluentAssertions;
    using Registration.Application.Services;
    using Registration.Domain.Interfaces;
    using Registration.Domain.Entities;
    using Microsoft.Extensions.Logging;
    using System.Text;
    using System.IO;
    using System.Threading.Tasks;

    public class RegistrationServiceTests
    {
        private readonly Mock<IVehicleRegistrationRepository> _repoMock = new();
        private readonly Mock<IUploadedFileRepository> _uploadedFileRepoMock = new();
        private readonly Mock<ILogger<RegistrationService>> _loggerMock = new();

        [Fact]
        public async Task ProcessUploadAsync_Should_Process_Valid_CSV()
        {
            // Arrange
            var service = new RegistrationService(_uploadedFileRepoMock.Object, _repoMock.Object, _loggerMock.Object);
            var csv = "Grantor First Name,Grantor Middle Names,Grantor Last Name,VIN,Registration start date,Registration duration,SPG ACN,SPG Organization Name\n" +
                      "Alice,,Smith,JH4DA3340GS000123,2025-01-01,7,001000004,Company A";

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

            _uploadedFileRepoMock.Setup(r => r.ExistsByHashAsync(It.IsAny<string>()))
                                .ReturnsAsync(false);

            _uploadedFileRepoMock.Setup(r => r.AddAsync(It.IsAny<string>()))
                                .Returns(Task.CompletedTask);

            _repoMock.Setup(r => r.FindAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                     .ReturnsAsync((VehicleRegistration?)null);

            _repoMock.Setup(r => r.AddAsync(It.IsAny<VehicleRegistration>()))
                     .Returns(Task.CompletedTask);

            _repoMock.Setup(r => r.SaveChangesAsync())
                     .Returns(Task.CompletedTask);

            // Act
            var result = await service.ProcessUploadAsync(stream);

            // Assert
            result.SubmittedRecords.Should().Be(1);
            result.ProcessedRecords.Should().Be(1);
            result.AddedRecords.Should().Be(1);
            result.InvalidRecords.Should().Be(0);
            result.UpdatedRecords.Should().Be(0);
        }
    }
}