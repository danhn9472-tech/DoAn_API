﻿using DoAn_API.DTOs;
using DoAn_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Collections.Generic;
using DoAn_API.Entities;

namespace DoAn_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserActivityController : ControllerBase
    {
        private readonly IUserActivityService _userActivityService;

        public UserActivityController(IUserActivityService userActivityService)
        {
            _userActivityService = userActivityService;
        }

        [HttpGet("my-recipe-book")]
        public async Task<IActionResult> GetMyRecipeBook()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _userActivityService.GetMyRecipeBookAsync(userId);
            return Ok(result);
        }

        [HttpGet("my-posts")]
        public async Task<IActionResult> GetMyPosts()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _userActivityService.GetMyPostsAsync(userId);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("profile/{userId}")]
        public async Task<IActionResult> GetUserProfile(string userId)
        {
            var result = await _userActivityService.GetUserProfileAsync(userId);
            return Ok(result);
        }
    }
}