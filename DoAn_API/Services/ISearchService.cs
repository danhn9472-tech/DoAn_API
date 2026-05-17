using DoAn_API.DTOs;
using System.Threading.Tasks;

namespace DoAn_API.Services
{
    public interface ISearchService
    {
        Task<SearchResultDto> SearchAsync(string keyword, int page, int pageSize);
    }
}