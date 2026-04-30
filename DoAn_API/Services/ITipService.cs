using DoAn_API.DTOs;
using System.Threading.Tasks;

namespace DoAn_API.Services
{
    public interface ITipService
    {
        Task<TipDTOs.PaginatedTipResponseDto> GetTipsAsync(int page, int pageSize);
        Task<TipDTOs.TipResponseDto> GetTipByIdAsync(int id);
        Task<int> CreateTipAsync(TipDTOs.CreateTipDto dto, string userId);
        Task UpdateTipAsync(int id, TipDTOs.CreateTipDto dto, string userId, bool isAdmin);
        Task DeleteTipAsync(int id, string userId, bool isAdmin);
    }
}