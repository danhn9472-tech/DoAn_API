﻿using DoAn_API.Services;
using DoAn_API.DTOs;
using DoAn_API.Entities;
using DoAn_API.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static DoAn_API.DTOs.CategoryDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DoAn_API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController: ControllerBase
    {
        private readonly IAdminService _adminService;
        
        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // Lấy danh sách các báo cáo bình luận đang chờ xử lý
        [HttpGet("comment-reports/pending")]
        public async Task<IActionResult> GetPendingCommentReports()
        {
            var reports = await _adminService.GetPendingCommentReportsAsync();
            return Ok(reports);
        }

        // Xử lý báo cáo bình luận (Ví dụ: Xóa bình luận và đánh dấu Resolved)
        [HttpPost("comment-reports/{reportId}/resolve")]
        public async Task<IActionResult> ResolveCommentReport(int reportId, [FromQuery] bool deleteComment, [FromQuery] bool banUser = false)
        {
            try
            {
                await _adminService.ResolveCommentReportAsync(reportId, deleteComment, banUser);
                return Ok(new { message = "Đã xử lý báo cáo." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // Lấy danh sách các báo cáo bài viết đang chờ xử lý
        [HttpGet("post-reports/pending")]
        public async Task<IActionResult> GetPendingPostReports()
        {
            var reports = await _adminService.GetPendingPostReportsAsync();
            return Ok(reports);
        }

        // Xử lý báo cáo bài viết (Ví dụ: Xóa bài viết và đánh dấu Resolved)
        [HttpPost("post-reports/{reportId}/resolve")]
        public async Task<IActionResult> ResolvePostReport(int reportId, [FromQuery] bool deletePost, [FromQuery] bool banUser = false)
        {
            try
            {
                await _adminService.ResolvePostReportAsync(reportId, deletePost, banUser);
                return Ok(new { message = "Đã xử lý báo cáo bài viết." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // Lấy danh sách tất cả người dùng
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _adminService.GetUsersAsync();
            return Ok(users);
        }

        // Khóa hoặc mở khóa tài khoản người dùng
        [HttpPost("users/{userId}/toggle-lockout")]
        public async Task<IActionResult> ToggleUserLockout(string userId)
        {
            try
            {
                await _adminService.ToggleUserLockoutAsync(userId);
                return Ok(new { message = "Cập nhật trạng thái khóa tài khoản thành công." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var stats = await _adminService.GetStatisticsAsync();
            return Ok(stats);
        }

        // ------DUYỆT RECIPE VÀ TIP------
        [HttpGet("pending-posts")]
        public async Task<IActionResult> GetPendingPosts()
        {
            var posts = await _adminService.GetPendingPostsAsync();
            return Ok(posts);
        }

        [HttpPost("approve-post")]
        public async Task<IActionResult> ApprovePost(int id, string type, int newStatus)
        {
            try
            {
                await _adminService.ApprovePostAsync(id, type, newStatus);
                return Ok(new { message = "Cập nhật trạng thái thành công!" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryTreeDto dto)
        {
            var result = await _adminService.CreateCategoryAsync(dto);
            return Ok(result);
        }

        // [PUT] api/Admin/categories/{id}
        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryTreeDto dto)
        {
            try
            {
                var result = await _adminService.UpdateCategoryAsync(id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
