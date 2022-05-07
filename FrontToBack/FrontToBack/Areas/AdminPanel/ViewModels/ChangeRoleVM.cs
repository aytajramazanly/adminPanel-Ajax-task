using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrontToBack.Areas.AdminPanel.ViewModels
{
    public class ChangeRoleVM
    {
        public List<IdentityRole> Roles { get; set; }
        public string CurrrentRole { get; set; }
        public string RoleId { get; set; }
        public string Username { get; set; }
    }
}
