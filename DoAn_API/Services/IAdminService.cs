using DoAn_API.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DoAn_API.Services
{
    public interface IAdminService
    {
        Task<IEnumerable<AdminDTOs.PendingReportDto>> GetPendingReportsAsync();
        Task ResolveReportAsync(int reportId, bool deleteComment);
    }
}