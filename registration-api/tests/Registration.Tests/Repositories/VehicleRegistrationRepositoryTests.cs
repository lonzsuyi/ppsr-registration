using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Registration.Domain.Entities;
using Registration.Domain.ValueObjects;
using Registration.Infrastructure.Persistence;
using Registration.Infrastructure.Repositories;
using Xunit;

namespace Registration.Tests.Repositories
{
    public class VehicleRegistrationRepositoryTests
    {
        private readonly ApplicationDbContext _context;
        private readonly VehicleRegistrationRepository _repository;

        public VehicleRegistrationRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new VehicleRegistrationRepository(_context, new Mock<ILogger<VehicleRegistrationRepository>>().Object);
        }

        [Fact]
        public async Task FindAsync_Should_Return_Null_When_Not_Found()
        {
            // Arrange
            var grantorFullName = "John Smith";
            var vin = "JH4DA3340GS000123";
            var spgAcn = "001000004";

            // Act
            var result = await _repository.FindAsync(grantorFullName, vin, spgAcn);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task FindAsync_Should_Return_Registration_When_Found()
        {
            // Arrange
            var registration = VehicleRegistration.Create(
                "John",
                "James",
                "Smith",
                "JH4DA3340GS000123",
                new DateTime(2025, 1, 1),
                RegistrationDuration.From("7 years"),
                "001000004",
                "Company A");

            await _context.VehicleRegistrations.AddAsync(registration);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.FindAsync("John James Smith", "JH4DA3340GS000123", "001000004");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(registration.Id, result.Id);
            Assert.Equal(registration.VIN, result.VIN);
        }

        [Fact]
        public async Task FindByVinAsync_Should_Return_Null_When_Not_Found()
        {
            // Act
            var result = await _repository.FindByVinAsync("JH4DA3340GS000123");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task FindByVinAsync_Should_Return_Registration_When_Found()
        {
            // Arrange
            var registration = VehicleRegistration.Create(
                "John",
                "James",
                "Smith",
                "JH4DA3340GS000123",
                new DateTime(2025, 1, 1),
                RegistrationDuration.From("7 years"),
                "001000004",
                "Company A");

            await _context.VehicleRegistrations.AddAsync(registration);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.FindByVinAsync("JH4DA3340GS000123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(registration.Id, result.Id);
            Assert.Equal(registration.VIN, result.VIN);
        }

        [Fact]
        public async Task AddAsync_Should_Add_Registration()
        {
            // Arrange
            var registration = VehicleRegistration.Create(
                "John",
                "James",
                "Smith",
                "JH4DA3340GS000123",
                new DateTime(2025, 1, 1),
                RegistrationDuration.From("7 years"),
                "001000004",
                "Company A");

            // Act
            await _repository.AddAsync(registration);
            await _repository.SaveChangesAsync();

            // Assert
            var result = await _context.VehicleRegistrations.FindAsync(registration.Id);
            Assert.NotNull(result);
            Assert.Equal(registration.Id, result.Id);
            Assert.Equal(registration.VIN, result.VIN);
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Registration()
        {
            // Arrange
            var registration = VehicleRegistration.Create(
                "John",
                "James",
                "Smith",
                "JH4DA3340GS000123",
                new DateTime(2025, 1, 1),
                RegistrationDuration.From("7 years"),
                "001000004",
                "Company A");

            await _context.VehicleRegistrations.AddAsync(registration);
            await _context.SaveChangesAsync();

            // Act
            registration.Update(
                "Jane",
                "Mary",
                "Smith",
                new DateTime(2025, 2, 1),
                RegistrationDuration.From("25 years"),
                "001000004",
                "Company A");

            await _repository.UpdateAsync(registration);
            await _repository.SaveChangesAsync();

            // Assert
            var result = await _context.VehicleRegistrations.FindAsync(registration.Id);
            Assert.NotNull(result);
            Assert.Equal("Jane", result.GrantorFirstName);
            Assert.Equal("Mary", result.GrantorMiddleNames);
            Assert.Equal("Smith", result.GrantorLastName);
            Assert.Equal(new DateTime(2025, 2, 1), result.RegistrationStartDate);
            Assert.Equal(RegistrationDuration.From("25 years"), result.Duration);
        }
    }
} 