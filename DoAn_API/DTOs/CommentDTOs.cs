﻿﻿﻿﻿﻿namespace DoAn_API.DTOs
{
    public class CommentDto
    {
        public string Content { get; set; }
        public int PostId { get; set; }
    }

    public class CommentResponseDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AuthorName { get; set; }
        public string? AuthorAvatarUrl { get; set; }
    }
}
