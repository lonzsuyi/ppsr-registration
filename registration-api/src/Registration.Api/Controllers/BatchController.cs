namespace Registration.Api.Controllers
{
    using System;
    using System.Security.Cryptography;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using Registration.Domain.Dtos;
    using Registration.Api.Dtos;
    using Registration.Application.Exceptions;
    using Registration.Api.Background;
    using Registration.Domain.Interfaces;

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class BatchController(
        IRegistrationService _service,
        IBackgroundTaskQueue _queue,
        IUploadedFileRepository _uploadedFileRepository,
        IUploadTaskStatusRepository _uploadTaskStatusRepository,
        IServiceProvider _serviceProvider,
        ILogger<BatchController> _logger) : ControllerBase
    {

        [HttpPost]
        [RequestSizeLimit(25_000_000)] // 25MB limit
        public async Task<ActionResult<UploadSummaryDto>> Upload([FromForm] BatchUploadRequest request)
        {
            if (request.File == null || request.File.Length == 0)
            {
                _logger.LogWarning("No file uploaded.");
                return BadRequest("No file uploaded.");
            }

            try
            {
                var result = await _service.ProcessUploadAsync(request.File.OpenReadStream());
                return Ok(result);
            }
            catch (ConflictException ex)
            {
                _logger.LogWarning(ex, "Duplicate file submission.");
                return Conflict(new ProblemDetails
                {
                    Title = "Duplicate submission",
                    Detail = ex.Message,
                    Status = StatusCodes.Status409Conflict
                });
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid CSV data",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in upload.");
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal server error",
                    Detail = "Unexpected error occurred during file processing."
                });
            }
        }

        [HttpPost]
        [RequestSizeLimit(25_000_000)] // 25MB
        public async Task<IActionResult> UploadBigFile([FromForm] BatchUploadRequest request)
        {
            if (request.File == null || request.File.Length == 0)
            {
                _logger.LogWarning("No file uploaded.");
                return BadRequest("No file uploaded.");
            }

            // Calculate hash of the file
            var file = request.File;
            string hash;
            using (var sha = SHA256.Create())
            using (var stream = file.OpenReadStream())
            {
                var hashBytes = await sha.ComputeHashAsync(stream);
                hash = Convert.ToBase64String(hashBytes);
            }

            // Check if the file already exists
            if (await _uploadedFileRepository.ExistsByHashAsync(hash))
            {
                _logger.LogWarning("Duplicate file upload detected: {Hash}", hash);
                return Conflict(new { message = "File already submitted previously." });
            }
            await _uploadedFileRepository.AddAsync(hash);



            // Queue the file for processing
            var task = await _uploadTaskStatusRepository.CreateAsync(hash);
            var memory = new MemoryStream();
            await file.CopyToAsync(memory);
            memory.Position = 0;

            await _queue.QueueBackgroundWorkItemAsync(async token =>
    {
        using var scope = _serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IRegistrationService>();
        var statusRepo = scope.ServiceProvider.GetRequiredService<IUploadTaskStatusRepository>();

        try
        {
            var summary = await service.ProcessUploadBigFileAsync(memory);
            await statusRepo.UpdateSummaryAsync(task.Id, summary);
            await statusRepo.MarkAsCompletedAsync(task.Id);
        }
        catch (Exception ex)
        {
            await statusRepo.MarkAsFailedAsync(task.Id, ex.Message);
        }
    });

            return Accepted(new { taskId = task.Id });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetStatus(Guid id)
        {
            var status = await _uploadTaskStatusRepository.GetByIdAsync(id);
            if (status == null)
                return NotFound(new { message = "Upload task not found." });

            return Ok(new
            {
                taskId = status.Id,
                status = status.Status,
                submitted = status.SubmittedRecords,
                processed = status.ProcessedRecords,
                invalid = status.InvalidRecords,
                added = status.AddedRecords,
                updated = status.UpdatedRecords,
                completedAt = status.CompletedAt,
                error = status.ErrorMessage
            });
        }
    }
}