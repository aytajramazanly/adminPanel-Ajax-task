using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FrontToBack.ViewModels.AccountViewModels
{
    public class ChangeEmailVM
    {
        [Required, EmailAddress, DataType(DataType.EmailAddress)]
        public string CurrentEmail { get; set; }
        [Required, EmailAddress, DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}
