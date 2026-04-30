﻿﻿﻿namespace DoAn_API.DTOs
{
    public class TipDTOs
    {
        public class CreateTipDto
        {
            public string Title { get; set; }
            public string Content { get; set; }
            public string? ImageUrl { get; set; }
        }

        public class TipResponseDto
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public string? ImageUrl { get; set; }
            public DateTime CreatedAt { get; set; }
            public int VoteCount { get; set; }
            public int SaveCount { get; set; }
            public string UserId { get; set; }
            public DoAn_API.Entities.Enums.PostStatus Status { get; set; }
            public string? AuthorName { get; set; }
        }

        public class PaginatedTipResponseDto
        {
            public IEnumerable<TipResponseDto> Data { get; set; }
            public int CurrentPage { get; set; }
            public int PageSize { get; set; }
            public int TotalItems { get; set; }
            public int TotalPages { get; set; }
        }
    }
}
