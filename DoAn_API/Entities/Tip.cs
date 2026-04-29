﻿using DoAn_API.Entities.Enums;
using DoAn_API.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAn_API.Entities
{
    public class Tip : Post
    {
        [Required]
        public string Content { get; set; }
    }
}
