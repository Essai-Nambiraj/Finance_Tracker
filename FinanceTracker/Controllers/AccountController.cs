using FinanceTracker.Data;
using FinanceTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Controllers
{
    public class AccountController : Controller
    {
        private readonly FTDbContext _context;
        private readonly User _user;

        public AccountController(FTDbContext context, User user)
        {
            _context = context;
            _user = user;
        }

        //Regiser GET
        public IActionResult Register()
        {
            return View();
        }

        //Register POST
        [HttpPost]
        public IActionResult Register(Users u)
        {
            _context.Users.Add(u);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        //Login GET
        public IActionResult Login()
        {
            return View();
        }

        //Login POST
        [HttpPost]
        public IActionResult Login(Users u)
        {
            var user = _context.Users.FirstOrDefault(x => x.Email == u.Email && x.Password == u.Password);

           

            if (user != null)
            {
                _user.UserName = user.Email;
                _user.UserId = user.User_Id;

                HttpContext.Session.SetString("UserEmail",user.Email.ToUpper());
                HttpContext.Session.SetInt32("UserId", user.User_Id);

                return RedirectToAction("Dashboard", "Transactions");
            }
            else
            {
                return RedirectToAction("Register", "Account");
            }
        }

        //Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}
