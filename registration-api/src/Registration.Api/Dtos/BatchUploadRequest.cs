using System.ComponentModel.DataAnnotations;

namespace Registration.Api.Dtos
{
public class BatchUploadRequest
{
    [Required]
    public IFormFile File { get; set; } = null!;
    }
}