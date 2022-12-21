using HermesChatApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using ServiceLayer;
using DataLayer;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;

namespace HermesChatApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly LoginOperations _loginOperator;
        private readonly IMapper _mapper;
        private readonly MailService _mailService;

        public HomeController(ILogger<HomeController> logger, LoginOperations loginOperator, IMapper map, MailService mailService)
        {
            _logger = logger;
            _loginOperator = loginOperator;
            _mapper = map;
            _mailService = mailService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Chat");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(UserModel userModel)
        {
            // check if username and password aren't empty
            if (userModel.Username == null || userModel.Password == null)
            {
                return View(userModel);
            }

            // check if user is valid
            var mappedUser = _mapper.Map<UserModel, User>(userModel);
            var validUser = _loginOperator.GetValidUser(mappedUser);
            if (validUser == null)
            {
                ViewBag.error = "Invalid Account";
                return View(userModel);
            }

            //authenticate user
            await _loginOperator.CreateAuthentication(validUser, HttpContext);
            return RedirectToAction("Index", "Chat");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // logging out user(removing cookie)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Chat");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(UserModel userModel)
        {
            if (ModelState.IsValid)
            {
                //check if user with username exists
                var mappedUser = _mapper.Map<UserModel, User>(userModel);
                var foundUserByUsername = _loginOperator.GetUserByUsername(mappedUser.Username);
                if (foundUserByUsername != null)
                {
                    ViewBag.error = "User with this name already exists";
                    return View(userModel);
                }
                else
                {
                    //adding new user to db
                    var newUser = _mapper.Map<UserModel, User>(userModel);
                    _loginOperator.Register(newUser);
                    ModelState.Clear();
                    TempData["SuccessfulRegister"] = "Person successfully created";
                    return RedirectToAction("Index");
                }
            }
            if (!ModelState.IsValid)
            {
                return Register();
            }
            return LocalRedirect("~/Home/Index");
        }

        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgetPassword(UserModel userModel)
        {
            // check if username and email aren't empty
            if (userModel.Username == null || userModel.Email == null)
            {
                ViewBag.error = "Required parameters are missing";
                return View(userModel);
            }

            Regex validateEmailRegex = new Regex("^\\S+@\\S+\\.\\S+$");
            if (!validateEmailRegex.IsMatch(userModel.Email))
            {
                ViewBag.error = "Email is invalid";
                return View(userModel);
            };

            // check if user is valid
            var mappedUser = _mapper.Map<UserModel, User>(userModel);
            var newRandomPassword = _loginOperator.GenerateNewPassword();
            var validUser = _loginOperator.UpdateUserPasswordByEmail(mappedUser, newRandomPassword);
            if (validUser == null)
            {
                ViewBag.error = "User not found";
                return View(userModel);
            }

            // send new password email
            _mailService.SendEmail(validUser, newRandomPassword);
            TempData["SuccessfulUpdate"] = "Email With New Password Sent";

            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Home");
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public IActionResult Profile()
        {
            var user = _loginOperator.GetUserByClaim(User);
            if (user == null)
            {
                ViewBag.error = "Please Logout. Account Error.";
                return RedirectToAction("Index", "Chat");
            }
            var userModel = _mapper.Map<User, UserModel>(user);
            return View(userModel);
        }

        [HttpGet]
        [Authorize]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult ResetPassword(string oldPswd, string new1Pswd, string new2Pswd)
        {
            // check if fields aren't empty
            if (oldPswd == null || new1Pswd == null || new2Pswd == null)
            {
                ViewBag.error = "Required parameters are missing";
                return View();
            }

            // check if new password is secure
            if (!_loginOperator.IsSecurePassword(new1Pswd))
            {
                ViewBag.error = "New password does not meet requirements!";
                return View();
            }

            // check if passwords are the same
            if (new1Pswd != new2Pswd)
            {
                ViewBag.error = "Passwords do not match!";
                return View();
            }

            // check if old password and new are two different
            if (oldPswd == new1Pswd)
            {
                ViewBag.error = "New and old passwords are similar";
                return View();
            }

            // get current user by claim
            var user = _loginOperator.GetUserByClaim(User);
            if (user == null)
            {
                ViewBag.error = "Please Logout. Account Error.";
                return View();
            }

            // check if old password match user password
            if (!BCrypt.Net.BCrypt.Verify(oldPswd, user.Password))
            {
                ViewBag.error = "Wrong Password. Don't remember password? Choose Forget Password option.";
                return View();
            }

            // update password and redirect
            _loginOperator.UpdateUserPassword(user, new1Pswd);
            TempData["ResetPassword"] = "Successful Password Change";
            return RedirectToAction("Profile", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}