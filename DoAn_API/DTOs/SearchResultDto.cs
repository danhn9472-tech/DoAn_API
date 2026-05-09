using System.Collections.Generic;

namespace DoAn_API.DTOs
{
    public class SearchResultDto
    {
        public List<RecipeDTOs.RecipeListItemDto> Recipes { get; set; } = new List<RecipeDTOs.RecipeListItemDto>();
        public List<TipDTOs.TipResponseDto> Tips { get; set; } = new List<TipDTOs.TipResponseDto>();
    }
}