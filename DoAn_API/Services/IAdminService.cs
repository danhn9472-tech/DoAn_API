using DoAn_API.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DoAn_API.Services
{
    public interface IAdminService
    {
        Task<IEnumerable<AdminDTOs.PendingReportDto>> GetPendingCommentReportsAsync();
        Task ResolveCommentReportAsync(int reportId, bool deleteComment, bool banUser = false);
        Task<IEnumerable<AdminDTOs.PendingPostReportDto>> GetPendingPostReportsAsync();
        Task ResolvePostReportAsync(int reportId, bool deletePost, bool banUser = false);
        Task<DashboardStatDto> GetStatisticsAsync();
        Task<IEnumerable<AdminDTOs.UserDto>> GetUsersAsync();
        Task ToggleUserLockoutAsync(string userId);
        Task<IEnumerable<PendingPostDto>> GetPendingPostsAsync();
        Task ApprovePostAsync(int id, string type, int newStatus);
        Task<CategoryDTOs.CategoryTreeDto> CreateCategoryAsync(CategoryDTOs.CategoryTreeDto dto);
        Task<CategoryDTOs.CategoryTreeDto> UpdateCategoryAsync(int id, CategoryDTOs.CategoryTreeDto dto);
    }
}