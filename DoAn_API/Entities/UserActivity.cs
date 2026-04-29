using System.ComponentModel.DataAnnotations;
﻿using System.ComponentModel.DataAnnotations.Schema;

namespace DoAn_API.Entities
{
    public class UserActivity
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Required]
        public int PostId { get; set; }
        
        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }

        public bool IsVoted { get; set; }
        public bool IsSaved { get; set; }
    }
}
