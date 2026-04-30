﻿using DoAn_API.DTOs;
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

        // -------UNIFIED VOTE ENDPOINT (recipe/tip)-------
        [HttpPost("vote/{itemType}/{itemId}")]
        public async Task<IActionResult> ToggleVote(string itemType, int itemId)
        {
            var validTypes = new[] { "recipe", "tip" };
            if (!validTypes.Contains(itemType.ToLower()))
            {
                return BadRequest(new { message = "Loại item không hợp lệ. Dùng 'recipe' hoặc 'tip'" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            try
            {
                var result = await _interactionService.ToggleVoteAsync(itemType.ToLower(), itemId, userId);
                return Ok(new { count = result.Count, status = result.Status });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // -------BÌNH LUẬN-------
        [HttpPost("comment")]
        public async Task<IActionResult> PostComment([FromBody] CommentDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var result = await _interactionService.PostCommentAsync(dto, userId);
            
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

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            try
            {
                var result = await _interactionService.ToggleSaveAsync(itemType.ToLower(), itemId, userId);
                return Ok(new { count = result.Count, status = result.Status });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GỬI BÁO CÁO BÌNH LUẬN
        [Authorize]
        [HttpPost("comment/{commentId}/report")]
        public async Task<IActionResult> ReportComment(int commentId, [FromBody] ReportDTOs.CreateReportDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            try
            {
                await _interactionService.ReportCommentAsync(commentId, dto.Reason, userId);
                return Ok(new { message = "Cảm ơn bạn đã báo cáo. Chúng tôi sẽ xem xét sớm nhất." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
