using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FrontToBack.ViewModels
{
    public class ResetPasswordVM
    {
        [Required, EmailAddress, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }

        [Required,DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required, DataType(DataType.Password),Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; }
    }
}
