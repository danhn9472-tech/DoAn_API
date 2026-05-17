using System.Collections.Generic;

namespace DoAn_API.DTOs
{
    public class SearchResultDto
    {
        public List<RecipeDTOs.RecipeListItemDto> Recipes { get; set; } = new List<RecipeDTOs.RecipeListItemDto>();
        public List<TipDTOs.TipResponseDto> Tips { get; set; } = new List<TipDTOs.TipResponseDto>();

        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalRecipes { get; set; }
        public int TotalTips { get; set; }
        public int TotalPages { get; set; }
    }
}