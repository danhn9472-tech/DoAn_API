﻿using System;

namespace DoAn_API.DTOs
{
    public class AdminDTOs
    {
        public class PendingReportDto
        {
            public int ReportId { get; set; }
            public string ReporterName { get; set; }
            public string Reason { get; set; }
            public string CommentContent { get; set; }
            public string CommentAuthor { get; set; }
            public string? CommentAuthorAvatarUrl { get; set; }
            public string AuthorId { get; set; }
            public DateTime ReportedAt { get; set; }
        }

        public class PendingPostReportDto
        {
            public int ReportId { get; set; }
            public string ReporterName { get; set; }
            public string Reason { get; set; }
            public string PostTitle { get; set; }
            public string PostType { get; set; }
            public string PostAuthor { get; set; }
            public string? PostAuthorAvatarUrl { get; set; }
            public string AuthorId { get; set; }
            public DateTime ReportedAt { get; set; }
            public int? RecipeId { get; set; }
            public int? TipId { get; set; }
        }

        public class UserDto
        {
            public string Id { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }
            public string FullName { get; set; }
            public string? AvatarUrl { get; set; }
            public bool IsLockedOut { get; set; }
            public DateTimeOffset? LockoutEnd { get; set; }
            public IEnumerable<string> Roles { get; set; }
        }
    }
}