using DoAn_API.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DoAn_API.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

            public DbSet<Recipe> Recipes { get; set; }
            public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
            public DbSet<RecipeStep> RecipeSteps { get; set; }
            public DbSet<Tip> Tips { get; set; }
            public DbSet<Comment> Comments { get; set; }
            public DbSet<UserActivity> UserActivities { get; set; }
            public DbSet<IngredientNutrition> IngredientNutritions { get; set; }
            public DbSet<CommentReport> CommentReports { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. Cấu hình cho bảng Comments (Tất cả chuyển về Restrict)
            builder.Entity<Comment>(entity =>
            {
                // Từ User -> Comment
                entity.HasOne(c => c.User)
                      .WithMany()
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Từ Recipe -> Comment
                entity.HasOne(c => c.Recipe)
                      .WithMany(r => r.Comments)
                      .HasForeignKey(c => c.RecipeId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Từ Tip -> Comment
                entity.HasOne(c => c.Tip)
                      .WithMany(t => t.Comments)
                      .HasForeignKey(c => c.TipId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 2. Cấu hình cho bảng UserActivities (Tất cả chuyển về NoAction)
            builder.Entity<UserActivity>(entity =>
            {
                entity.HasOne(ua => ua.User)
                      .WithMany()
                      .HasForeignKey(ua => ua.UserId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ua => ua.Recipe)
                      .WithMany()
                      .HasForeignKey(ua => ua.RecipeId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ua => ua.Tip)
                      .WithMany()
                      .HasForeignKey(ua => ua.TipId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            //report comment
            builder.Entity<CommentReport>(entity =>
            {
                entity.HasOne(cr => cr.User)
                      .WithMany()
                      .HasForeignKey(cr => cr.UserId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cr => cr.Comment)
                      .WithMany()
                      .HasForeignKey(cr => cr.CommentId)
                      .OnDelete(DeleteBehavior.NoAction); 
            });
        }
    }
}
