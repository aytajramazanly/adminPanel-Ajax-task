using FrontToBack.Models;
using FrontToBack.ViewModels;
using FrontToBack.ViewModels.AccountViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace FrontToBack.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;

        private readonly SignInManager<User> _signInManager;
        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
                return View();

            var existUser = await _userManager.FindByNameAsync(registerVM.Username);
            if (existUser != null)
            {
                ModelState.AddModelError("Username", "This Username already exist!");
                return View();
            }

            var user = new User
            {
                Fullname = registerVM.Fullname,
                UserName = registerVM.Username,
                Email = registerVM.Email,
                IsActive=true
            };

            var result = await _userManager.CreateAsync(user, registerVM.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View();
            }

            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string link = Url.Action("ConfirmEmail", "Account", new { email = user.Email, token }, Request.Scheme, Request.Host.ToString());

            MailMessage message = new MailMessage();
            message.From = new MailAddress("codep320@gmail.com", "Fiorella");
            message.To.Add(user.Email);
            string body = string.Empty;
            using (StreamReader streamReader=new StreamReader("wwwroot/Templates/confirmEmail.html"))
            {
               body = streamReader.ReadToEnd();
            }
            message.IsBodyHtml = true;
            message.Body = body.Replace("{{link}}", link);
            message.Subject = "Confirm Email";

            SmtpClient smtp = new SmtpClient();
            smtp.Port = 587;
            smtp.Host = "smtp.gmail.com";
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential("codep320@gmail.com", "codeacademyp320");
            smtp.Send(message);
            TempData["MessageBox"] = "Register";
            return RedirectToAction(nameof(Index), "Home");
        }

        public async Task<IActionResult> ConfirmEmail(string email,string token)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return BadRequest();
            await _userManager.ConfirmEmailAsync(user, token);
            await _signInManager.SignInAsync(user, false);

            TempData["MessageBox"] = "ConfirmEmail";
            return RedirectToAction(nameof(Index), "Home");
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
                return View();

            var user = await _userManager.FindByNameAsync(loginVM.Username);
            if (user == null)
            {
                ModelState.AddModelError("", "Incorrect Credentials!");
                return View();
            }
            if (user.IsActive == false)
            {
                ModelState.AddModelError("", "This user has been deactivated");
                return View();
            }
            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.RememberMe, true);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Incorrect Credentials!");
                return View();
            }
            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Index), "Home");
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return BadRequest();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            string link = Url.Action(nameof(ResetPassword), "Account", new { email = user.Email, token }, Request.Scheme, Request.Host.ToString());

            MailMessage message = new MailMessage();
            message.From = new MailAddress("codep320@gmail.com","Fiorella");
            message.To.Add(user.Email);
            message.IsBodyHtml = true;
            string body = string.Empty;
            using (StreamReader streamReader = new StreamReader("wwwroot/Templates/resetpassword.html"))
            {
                body = streamReader.ReadToEnd();
            }
            message.Body = body.Replace("{{link}}", link);
            message.Subject = "Reset Password";

            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.Credentials = new NetworkCredential("codep320@gmail.com", "codeacademyp320");
            smtp.EnableSsl = true;
            smtp.Send(message);

            TempData["MessageBox"] = "ForgotPassword";
            return RedirectToAction(nameof(Index), "Home");
        }

        public IActionResult ResetPassword(string email,string token)
        {
            return View(new ResetPasswordVM { 
             Email=email,
             Token=token
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM resetPasswordVM)
        {
            if (!ModelState.IsValid)
                return View();
            var user = await _userManager.FindByEmailAsync(resetPasswordVM.Email);
            if (user == null)
                return BadRequest();
        
            var result=await _userManager.ResetPasswordAsync(user, resetPasswordVM.Token, resetPasswordVM.NewPassword);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Reset Password Failed");
                return View();
            }
            TempData["MessageBox"] = "ResetPassword";
            return RedirectToAction(nameof(Login));
        }

        public async Task<IActionResult> ChangePassword(string username)
        {
            if (username == null)
                return NotFound();
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return NotFound();
            return View(new ChangePasswordVM { 
            Username=user.UserName
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            if (!ModelState.IsValid)
                return View();
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
                return NotFound();

            var checkCurrentPassword = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
            if (checkCurrentPassword == false)
            {
                ModelState.AddModelError("CurrentPassword", "Please enter the current password correctly");
                return View();
            }

            var checkPasswordsIsSame =  _userManager.PasswordHasher.VerifyHashedPassword(user,user.PasswordHash,model.NewPassword);
            if (checkPasswordsIsSame == PasswordVerificationResult.Success)
            {
                ModelState.AddModelError("NewPassword", "Your new and old password cannot be the same");
                return View();
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }
                return View();
            }

            await _userManager.UpdateSecurityStampAsync(user);
            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(user,false);
            TempData["MessageBox"] = "ChangePassword";
            return RedirectToAction("UserProfile", new { username = user.UserName });
        }

        public async Task<IActionResult> ChangeEmail(string username)
        {
            if (username == null)
                return NotFound();
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return NotFound();

            return View(new ChangeEmailVM
            {
              CurrentEmail=user.Email,
               Email=user.Email
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeEmail(ChangeEmailVM changeEmailVM)
        {
            if (!ModelState.IsValid)
                return View(changeEmailVM);

            var user = await _userManager.FindByEmailAsync(changeEmailVM.CurrentEmail);
            if (user == null)
                return NotFound();
            
            if (user.Email == changeEmailVM.Email)
            {
                ModelState.AddModelError("Email", "This Email address already used in your account");
                return View(changeEmailVM);
            }

            var existUser = await _userManager.FindByEmailAsync(changeEmailVM.Email);
            if (existUser != null)
            {
                ModelState.AddModelError("Email", "This Email address already used");
                return View(changeEmailVM);
            }
            string token = await _userManager.GenerateChangeEmailTokenAsync(user,changeEmailVM.Email);
            string link = Url.Action("ConfirmChangeEmail", "Account", new { currentEmail=user.Email, newEmail = changeEmailVM.Email, token }, Request.Scheme, Request.Host.ToString());

            MailMessage message=new MailMessage();
            message.From = new MailAddress("codep320@gmail.com", "Fiorella");
            message.To.Add(changeEmailVM.Email);
            string body = string.Empty;
            using (StreamReader streamReader = new StreamReader("wwwroot/Templates/confirmEmail.html"))
            {
                body = streamReader.ReadToEnd();
            }
            message.IsBodyHtml = true;
            message.Body = body.Replace("{{link}}", link);
            message.Subject = "Confirm Change Email";

            SmtpClient smtp = new SmtpClient();
            smtp.Port = 587;
            smtp.Host = "smtp.gmail.com";
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential("codep320@gmail.com", "codeacademyp320");
            smtp.Send(message);
            TempData["MessageBox"] = "ChangeEmail";
            return RedirectToAction("UserProfile", new { username = user.UserName });
        }
        public async Task<IActionResult> ConfirmChangeEmail(string currentEmail,string newEmail, string token)
        {
            var user = await _userManager.FindByEmailAsync(currentEmail);
            if (user == null)
                return BadRequest();
            await _userManager.ChangeEmailAsync(user, newEmail,token);

            await _userManager.UpdateSecurityStampAsync(user);
            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(user, false);
            TempData["MessageBox"] = "ConfirmChangeEmail";
            return RedirectToAction("UserProfile", new { username = user.UserName });
        }

        public async Task<IActionResult> UserProfile(string username)
        {
            if (username == null)
                return NotFound();
            var user = await _userManager.FindByNameAsync(username);
            if(user==null)
                return NotFound();
            return View(user);
        }
    }
}
