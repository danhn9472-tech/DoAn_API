using DoAn_API.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DoAn_API.Services
{
    public interface IInteractionService
    {
        Task<ToggleResultDto> ToggleVoteAsync(string itemType, int itemId, string userId);
        Task<CommentResponseDto> PostCommentAsync(CommentDto dto, string userId);
        Task<IEnumerable<CommentResponseDto>> GetCommentsAsync(int itemId);
        Task<ToggleResultDto> ToggleSaveAsync(string itemType, int itemId, string userId);
        Task ReportCommentAsync(int commentId, string reason, string userId);
        Task ReportPostAsync(string itemType, int itemId, string reason, string userId);
    }
}