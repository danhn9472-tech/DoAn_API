using Microsoft.AspNetCore.Identity; 
namespace DoAn_API.Entities
{
    public class ApplicationUser: IdentityUser
    {
        public string? FullName { get; set; }
        public virtual ICollection<Recipe> Recipes { get; set; }
    }
}
