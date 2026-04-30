using DoAn_API.DTOs;
using System.Threading.Tasks;

namespace DoAn_API.Services
{
    public interface IUserActivityService
    {
        Task<SavedItemsDto> GetMyRecipeBookAsync(string userId);
        Task<MyPostsDto> GetMyPostsAsync(string userId);
    }
}