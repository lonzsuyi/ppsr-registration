using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Registration.Api.Background;
using Registration.Api.Controllers;
using Registration.Api.Dtos;
using Registration.Application.Exceptions;
using Registration.Domain.Dtos;
using Registration.Domain.Entities;
using Registration.Domain.Interfaces;
using System.Text;
using Xunit;

namespace Registration.Tests.Controllers
{
    public class BatchControllerTests
    {
        private readonly Mock<IRegistrationService> _serviceMock;
        private readonly Mock<IBackgroundTaskQueue> _queueMock;
        private readonly Mock<IUploadedFileRepository> _uploadedFileRepoMock;
        private readonly Mock<IUploadTaskStatusRepository> _uploadTaskStatusRepoMock;
        private readonly Mock<ILogger<BatchController>> _loggerMock;
        private readonly IServiceProvider _serviceProvider;
        private readonly BatchController _controller;

        public BatchControllerTests()
        {
            _serviceMock = new Mock<IRegistrationService>();
            _queueMock = new Mock<IBackgroundTaskQueue>();
            _uploadedFileRepoMock = new Mock<IUploadedFileRepository>();
            _uploadTaskStatusRepoMock = new Mock<IUploadTaskStatusRepository>();
            _loggerMock = new Mock<ILogger<BatchController>>();

            var services = new ServiceCollection();
            services.AddScoped(_ => _serviceMock.Object);
            _serviceProvider = services.BuildServiceProvider();

            _controller = new BatchController(
                _serviceMock.Object,
                _queueMock.Object,
                _uploadedFileRepoMock.Object,
                _uploadTaskStatusRepoMock.Object,
                _serviceProvider,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Upload_Should_Return_BadRequest_When_No_File()
        {
            // Arrange
            var request = new BatchUploadRequest();

            // Act
            var result = await _controller.Upload(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("No file uploaded.", badRequestResult.Value);
        }

        [Fact]
        public async Task Upload_Should_Process_Valid_File()
        {
            // Arrange
            var content = "col1,col2\nvalue1,value2";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var file = new FormFile(stream, 0, content.Length, "Data", "test.csv");
            var request = new BatchUploadRequest { File = file };
            var summary = new UploadSummaryDto { SubmittedRecords = 1, ProcessedRecords = 1 };

            _serviceMock.Setup(s => s.ProcessUploadAsync(It.IsAny<Stream>()))
                .ReturnsAsync(summary);

            // Act
            var result = await _controller.Upload(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedSummary = Assert.IsType<UploadSummaryDto>(okResult.Value);
            Assert.Equal(1, returnedSummary.SubmittedRecords);
            Assert.Equal(1, returnedSummary.ProcessedRecords);
        }

        [Fact]
        public async Task Upload_Should_Return_Conflict_For_Duplicate_File()
        {
            // Arrange
            var content = "col1,col2\nvalue1,value2";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var file = new FormFile(stream, 0, content.Length, "Data", "test.csv");
            var request = new BatchUploadRequest { File = file };

            _serviceMock.Setup(s => s.ProcessUploadAsync(It.IsAny<Stream>()))
                .ThrowsAsync(new ConflictException("File has already been submitted previously"));

            // Act
            var result = await _controller.Upload(request);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
            var problemDetails = Assert.IsType<ProblemDetails>(conflictResult.Value);
            Assert.Equal(409, problemDetails.Status);
            Assert.Equal("Duplicate submission", problemDetails.Title);
        }

        [Fact]
        public async Task UploadBigFile_Should_Return_BadRequest_When_No_File()
        {
            // Arrange
            var request = new BatchUploadRequest();

            // Act
            var result = await _controller.UploadBigFile(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No file uploaded.", badRequestResult.Value);
        }

        [Fact]
        public async Task UploadBigFile_Should_Queue_File_For_Processing()
        {
            // Arrange
            var content = "col1,col2\nvalue1,value2";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var file = new FormFile(stream, 0, content.Length, "Data", "test.csv");
            var request = new BatchUploadRequest { File = file };
            var taskId = Guid.NewGuid();

            _uploadedFileRepoMock.Setup(r => r.ExistsByHashAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _uploadTaskStatusRepoMock.Setup(r => r.CreateAsync(It.IsAny<string>()))
                .ReturnsAsync(new UploadTaskStatus { Id = taskId });

            // Act
            var result = await _controller.UploadBigFile(request);

            // Assert
            var acceptedResult = Assert.IsType<AcceptedResult>(result);
            var response = Assert.IsType<dynamic>(acceptedResult.Value);
            Assert.Equal(taskId, response.taskId);
        }

        [Fact]
        public async Task GetStatus_Should_Return_NotFound_For_Invalid_TaskId()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            _uploadTaskStatusRepoMock.Setup(r => r.GetByIdAsync(taskId))
                .ReturnsAsync((UploadTaskStatus?)null);

            // Act
            var result = await _controller.GetStatus(taskId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<dynamic>(notFoundResult.Value);
            Assert.Equal("Upload task not found.", response.message);
        }

        [Fact]
        public async Task GetStatus_Should_Return_Task_Status()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var status = new UploadTaskStatus
            {
                Id = taskId,
                Status = "Completed",
                SubmittedRecords = 10,
                ProcessedRecords = 10,
                InvalidRecords = 0,
                AddedRecords = 8,
                UpdatedRecords = 2,
                CompletedAt = DateTime.UtcNow
            };

            _uploadTaskStatusRepoMock.Setup(r => r.GetByIdAsync(taskId))
                .ReturnsAsync(status);

            // Act
            var result = await _controller.GetStatus(taskId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<dynamic>(okResult.Value);
            Assert.Equal(taskId, response.taskId);
            Assert.Equal("Completed", response.status);
            Assert.Equal(10, response.submitted);
            Assert.Equal(10, response.processed);
            Assert.Equal(0, response.invalid);
            Assert.Equal(8, response.added);
            Assert.Equal(2, response.updated);
        }
    }
} 