using FrontToBack.Areas.AdminPanel.ViewModels;
using FrontToBack.Data;
using FrontToBack.DataAccessLayer;
using FrontToBack.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrontToBack.Areas.AdminPanel.Controllers
{
    [Area("AdminPanel")]
    public class UserController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(AppDbContext dbContext, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            if (page < 1)
                return NotFound();
            if (((page - 1) * 10) >= await _dbContext.Users.CountAsync())
                page--;
            var totalPageCount = Math.Ceiling((decimal)await _dbContext.Users.CountAsync() / 10);
            if (page > totalPageCount)
                return NotFound();

            ViewBag.totalPageCount = totalPageCount;
            ViewBag.currentPage = page;
            var users = await _dbContext.Users.Skip((page - 1) * 10).Take(10).ToListAsync();
            var adminRole = await _roleManager.FindByNameAsync("Admin");
            var userRoles = await _dbContext.UserRoles.Where(x => x.RoleId != adminRole.Id).ToListAsync();
            List<User> newUsers=new List<User>();
            foreach (var user in users)
            {
                var IsNotAdmin = userRoles.Any(x => x.UserId == user.Id);
                if (IsNotAdmin)
                    newUsers.Add(user);
                else
                    continue;
            }
            return View(newUsers);
        }

        public async Task<IActionResult> ChangeActivation(string username)
        {
            if (username == null)
                return BadRequest();
            var user = await _userManager.FindByNameAsync(username);
            if (user==null)
                return NotFound();

            if (user.IsActive)
                user.IsActive = false;
            else
                user.IsActive = true;
            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }
        //public async Task<IActionResult> ChangeRole(string username)
        //{
        //    if (username == null)
        //        return BadRequest();
        //    var user = await _userManager.FindByNameAsync(username);
        //    if (user == null)
        //        return NotFound();

        //    string currentRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
        //    if (currentRole == null)
        //        return NotFound();
        //    var roles=await _roleManager.Roles.ToListAsync();
        //    ViewBag.CurrentRole = currentRole;

        //    return View(roles); 
        //}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ChangeRole(string username,string role)
        //{
        //    if (username == null && role==null)
        //        return BadRequest();
        //    var user = await _userManager.FindByNameAsync(username);
        //    if (user == null)
        //        return NotFound();

        //    var existRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();


        //    if (existRole == null)
        //        return BadRequest();
        //    var roles = await _roleManager.Roles.ToListAsync();
        //    ViewBag.CurrentRole = existRole;
        //    if (existRole == role)
        //    {
        //        ModelState.AddModelError("", "This is current Role");
        //        return View(roles);
        //    }
        //    var removeResult = await _userManager.RemoveFromRoleAsync(user, existRole);
        //    if (!removeResult.Succeeded)
        //    {
        //        foreach (var item in removeResult.Errors)
        //        {
        //            ModelState.AddModelError("", item.Description);
        //        }
        //        return View(roles);
        //    }

        //    var addResult = await _userManager.AddToRoleAsync(user, role);
        //    if (!addResult.Succeeded)
        //    {
        //        foreach (var item in removeResult.Errors)
        //        {
        //            ModelState.AddModelError("", item.Description);
        //        }
        //        return View(roles);
        //    }

        //    return RedirectToAction(nameof(Index));
        //}


        public async Task<IActionResult> ChangeRole(string username)
        {
            if (username == null)
                return BadRequest();
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return NotFound();

            var currentRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            if (currentRole == null)
                return NotFound();
            var roles = await _roleManager.Roles.ToListAsync();
            return View(new ChangeRoleVM { 
                Roles=roles,
                CurrrentRole=currentRole,
                Username=user.UserName
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(ChangeRoleVM changeRoleVM)
        {
            if (!ModelState.IsValid)
                return View();
            var user = await _userManager.FindByNameAsync(changeRoleVM.Username);
            if (user == null)
                return NotFound();

            changeRoleVM.Roles = await _roleManager.Roles.ToListAsync();

            if (changeRoleVM.RoleId=="0")
            {
                ModelState.AddModelError("", "Please Select one");
                return View(changeRoleVM);
            }

            var newRole=await _roleManager.FindByIdAsync(changeRoleVM.RoleId);
            if (newRole.Name == changeRoleVM.CurrrentRole)
            {
                ModelState.AddModelError("", "This is current Role");
                return View(changeRoleVM);
            }

            var addResult = await _userManager.AddToRoleAsync(user, newRole.Name);
            if (!addResult.Succeeded)
            {
                foreach (var item in addResult.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }
                return View(changeRoleVM);
            }

            var removeResult = await _userManager.RemoveFromRoleAsync(user, changeRoleVM.CurrrentRole);
            if (!removeResult.Succeeded)
            {
                foreach (var item in removeResult.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }
                return View(changeRoleVM);
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
