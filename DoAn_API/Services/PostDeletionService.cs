using DoAn_API.Data;
using DoAn_API.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DoAn_API.Services
{
    public class PostDeletionService : IPostDeletionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUploadService _uploadService;

        public PostDeletionService(ApplicationDbContext context, IUploadService uploadService)
        {
            _context = context;
            _uploadService = uploadService;
        }

        public void QueueFullPostDeletion(Post post)
        {
            if (post == null) return;

            // --- SOFT DELETE ---
            post.IsDeleted = true;
            post.DeletedAt = System.DateTime.UtcNow;
            
            /*
            // 1. Queue UserActivities deletion
            var activities = _context.UserActivities.Where(ua => ua.PostId == post.Id);
            _context.UserActivities.RemoveRange(activities);

            // 2. Queue Comments deletion (Caller must Include this)
            if (post.Comments != null && post.Comments.Any())
            {
                _context.Comments.RemoveRange(post.Comments);
            }

            // 3. Queue PostReports deletion
            var reports = _context.PostReports.Where(pr => pr.RecipeId == post.Id || pr.TipId == post.Id);
            _context.PostReports.RemoveRange(reports);

            // 4. Delete cover image
            if (!string.IsNullOrEmpty(post.ImageUrl))
            {
                _uploadService.DeleteImage(post.ImageUrl);
            }

            // 5. Recipe-specific: Delete step images (Caller must Include this)
            if (post is Recipe recipe && recipe.RecipeSteps != null)
            {
                foreach (var step in recipe.RecipeSteps)
                {
                    if (!string.IsNullOrEmpty(step.ImageUrl))
                    {
                        _uploadService.DeleteImage(step.ImageUrl);
                    }
                }
            }

            // Sử dụng _context.Remove() thay vì _context.Posts.Remove() để tránh lỗi nếu DbContext không có DbSet<Post>
            _context.Remove(post);
            */
        }

        public void RestorePost(Post post)
        {
            if (post == null) return;
            post.IsDeleted = false;
            post.DeletedAt = null;
        }
    }
}