﻿﻿﻿using DoAn_API.DTOs;
using DoAn_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DoAn_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InteractionController : ControllerBase
    {
        private readonly IInteractionService _interactionService;
        
        public InteractionController(IInteractionService interactionService)
        {
            _interactionService = interactionService;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // -------UNIFIED VOTE ENDPOINT (recipe/tip)-------
        [HttpPost("vote/{itemType}/{itemId}")]
        public async Task<IActionResult> ToggleVote(string itemType, int itemId)
        {
            var validTypes = new[] { "recipe", "tip" };
            if (!validTypes.Contains(itemType.ToLower()))
            {
                return BadRequest(new { message = "Loại item không hợp lệ. Dùng 'recipe' hoặc 'tip'" });
            }

            if (string.IsNullOrEmpty(CurrentUserId)) return Unauthorized();
            
            var result = await _interactionService.ToggleVoteAsync(itemType.ToLower(), itemId, CurrentUserId);
            return Ok(new { count = result.Count, status = result.Status });
        }

        // -------BÌNH LUẬN-------
        [HttpPost("comment")]
        public async Task<IActionResult> PostComment([FromBody] CommentDto dto)
        {
            if (string.IsNullOrEmpty(CurrentUserId)) return Unauthorized();
            
            var result = await _interactionService.PostCommentAsync(dto, CurrentUserId);
            
            return Ok(new {
                id = result.Id,
                content = result.Content,
                createdAt = result.CreatedAt
            });
        }

        //------COI BÌNH LUẬN CỦA CÔNG THỨC----
        [AllowAnonymous]
        [HttpGet("recipe/{recipeId}/comments")]
        public async Task<IActionResult> GetRecipeComments(int recipeId)
        {
            var comments = await _interactionService.GetCommentsAsync(recipeId);
            return Ok(comments);
        }

        // ------COI BÌNH LUẬN------
        [AllowAnonymous]
        [HttpGet("comments/{itemType}/{itemId}")]
        public async Task<IActionResult> GetComments(string itemType, int itemId)
        {
            var validTypes = new[] { "recipe", "tip" };
            if (!validTypes.Contains(itemType.ToLower()))
            {
                return BadRequest(new { message = "Loại item không hợp lệ. Dùng 'recipe' hoặc 'tip'" });
            }

            var comments = await _interactionService.GetCommentsAsync(itemId);
            return Ok(comments);
        }

        // -------UNIFIED SAVE ENDPOINT (recipe/tip)-------
        [HttpPost("save/{itemType}/{itemId}")]
        public async Task<IActionResult> ToggleSave(string itemType, int itemId)
        {
            var validTypes = new[] { "recipe", "tip" };
            if (!validTypes.Contains(itemType.ToLower()))
            {
                return BadRequest(new { message = "Loại item không hợp lệ. Dùng 'recipe' hoặc 'tip'" });
            }

            if (string.IsNullOrEmpty(CurrentUserId)) return Unauthorized();
            
            var result = await _interactionService.ToggleSaveAsync(itemType.ToLower(), itemId, CurrentUserId);
            return Ok(new { count = result.Count, status = result.Status });
        }

        // GỬI BÁO CÁO BÌNH LUẬN
        [Authorize]
        [HttpPost("comment/{commentId}/report")]
        public async Task<IActionResult> ReportComment(int commentId, [FromBody] ReportDTOs.CreateReportDto dto)
        {
            if (string.IsNullOrEmpty(CurrentUserId)) return Unauthorized();
            
            await _interactionService.ReportCommentAsync(commentId, dto.Reason, CurrentUserId);
            return Ok(new { message = "Cảm ơn bạn đã báo cáo. Chúng tôi sẽ xem xét sớm nhất." });
        }

        // GỬI BÁO CÁO BÀI VIẾT (RECIPE / TIP)
        [Authorize]
        [HttpPost("report/{itemType}/{itemId}")]
        public async Task<IActionResult> ReportPost(string itemType, int itemId, [FromBody] ReportDTOs.CreateReportDto dto)
        {
            var validTypes = new[] { "recipe", "tip" };
            if (!validTypes.Contains(itemType.ToLower()))
            {
                return BadRequest(new { message = "Loại item không hợp lệ. Dùng 'recipe' hoặc 'tip'" });
            }

            if (string.IsNullOrEmpty(CurrentUserId)) return Unauthorized();
            
            await _interactionService.ReportPostAsync(itemType.ToLower(), itemId, dto.Reason, CurrentUserId);
            return Ok(new { message = "Cảm ơn bạn đã báo cáo. Chúng tôi sẽ xem xét sớm nhất." });
        }
    }
}
