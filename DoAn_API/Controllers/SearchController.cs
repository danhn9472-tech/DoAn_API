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

        // GET: api/Search?keyword=pho
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            var results = await _searchService.SearchAsync(keyword);
            return Ok(results);
        }
    }
}