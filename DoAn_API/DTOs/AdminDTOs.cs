using System;

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
            public DateTime ReportedAt { get; set; }
        }
    }
}