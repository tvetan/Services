﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloggingSystem.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        //unique
        [Required]
        [MinLength(6), MaxLength(30)]
        public string Username { get; set; }

        //unique
        [Required]
        [MinLength(6),MaxLength(30)]
        public string DisplayName { get; set; }

        [Required]
        public string AuthCode { get; set; }

        public string SessionKey { get; set; }

        public virtual ICollection<Post> Posts { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

        public User()
        {
            this.Posts = new HashSet<Post>();
            this.Comments = new HashSet<Comment>();
        }


    }
}
