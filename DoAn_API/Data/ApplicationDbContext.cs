﻿using DoAn_API.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DoAn_API.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

            public DbSet<Post> Posts { get; set; }
            public DbSet<Recipe> Recipes { get; set; }
            public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
            public DbSet<RecipeStep> RecipeSteps { get; set; }
            public DbSet<Tip> Tips { get; set; }
            public DbSet<Comment> Comments { get; set; }
            public DbSet<UserActivity> UserActivities { get; set; }
            public DbSet<IngredientNutrition> IngredientNutritions { get; set; }
            public DbSet<CommentReport> CommentReports { get; set; }
            public DbSet<Category> Categories { get; set; }
            public DbSet<RecipeCategory> RecipeCategories { get; set; }
            public DbSet<PostReport> PostReports { get; set; }
            public DbSet<Notification> Notifications { get; set; } // Thêm bảng Thông báo

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Thiết lập Kế thừa (TPH - Table Per Hierarchy)
            builder.Entity<Post>()
                .HasDiscriminator<string>("PostType")
                .HasValue<Recipe>("Recipe")
                .HasValue<Tip>("Tip");

            // 1. Cấu hình cho bảng Comments (Tất cả chuyển về Restrict)
            builder.Entity<Comment>(entity =>
            {
                // Từ User -> Comment
                entity.HasOne(c => c.User)
                      .WithMany()
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Từ Post -> Comment
                entity.HasOne(c => c.Post)
                      .WithMany(p => p.Comments)
                      .HasForeignKey(c => c.PostId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // 2. Cấu hình cho bảng UserActivities (Tất cả chuyển về NoAction)
            builder.Entity<UserActivity>(entity =>
            {
                entity.HasOne(ua => ua.User)
                      .WithMany()
                      .HasForeignKey(ua => ua.UserId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ua => ua.Post)
                      .WithMany(p => p.Activities)
                      .HasForeignKey(ua => ua.PostId)
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

            builder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict); // Không cho xóa cha nếu còn con

            builder.Entity<RecipeCategory>()
                .HasKey(rc => new { rc.RecipeId, rc.CategoryId });

            builder.Entity<RecipeCategory>()
                .HasOne(rc => rc.Recipe)
                .WithMany(r => r.RecipeCategories)
                .HasForeignKey(rc => rc.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RecipeCategory>()
                .HasOne(rc => rc.Category)
                .WithMany(c => c.RecipeCategories)
                .HasForeignKey(rc => rc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
