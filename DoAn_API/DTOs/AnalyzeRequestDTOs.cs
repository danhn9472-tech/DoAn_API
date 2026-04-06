using static DoAn_API.DTOs.RecipeDTOs;
using System.Net.Http.Json;

namespace DoAn_API.DTOs
{
    public class AnalyzeRequestDTOs
    {
        public List<IngredientDto> Ingredients { get; set; }
    }
}
