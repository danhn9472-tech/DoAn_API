using System.ComponentModel.DataAnnotations;

namespace DoAn_API.DTOs
{
    public class ReportDTOs
    {
        public class CreateReportDto
        {
            [Required]
            public string Reason { get; set; }
        }
    }
}
