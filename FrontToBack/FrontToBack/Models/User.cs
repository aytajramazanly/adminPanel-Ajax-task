using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FrontToBack.Models
{
    public class User: IdentityUser
    {
        [Required]
        public string Fullname { get; set; }
        [Required]
        public bool IsActive { get; set; } = true;
    }
}
