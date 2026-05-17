using DoAn_API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DoAn_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var results = await _searchService.SearchAsync(keyword ?? string.Empty, page, pageSize);
            return Ok(results);
        }
    }
}