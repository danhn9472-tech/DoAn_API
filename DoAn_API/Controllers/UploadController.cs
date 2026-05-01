using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DoAn_API.Services;

namespace DoAn_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Chỉ cho phép user đã đăng nhập được upload file
    public class UploadController : ControllerBase
    {
        private readonly IUploadService _uploadService;

        public UploadController(IUploadService uploadService)
        {
            _uploadService = uploadService;
        }

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            var imageUrl = await _uploadService.UploadImageAsync(file);
            
            return Ok(new { message = "Upload thành công!", imageUrl = imageUrl });
        }
    }
}
