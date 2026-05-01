using Microsoft.AspNetCore.Http;

namespace DoAn_API.Services
{
    public interface IUploadService
    {
        Task<string> UploadImageAsync(IFormFile file);
        void DeleteImage(string imageUrl);
    }
}