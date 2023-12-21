using Microsoft.AspNetCore.Mvc;
using Watcher.WebApi.Classes;

namespace WatcherLogin.Controllers
{
    public class LoginController : Controller
    {
       private readonly Context _context;

        public LoginController(Context context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string username, string password)
        {
            var values = _context.Admins.FirstOrDefault(x => x.Username == username && x.Password == password);
            if (values == null)
            {
                return RedirectToAction("Index", "http://localhost:5145/api/Watcher/status");

            }
            else
            {
                return RedirectToAction("Index");
            }
        }
            
    }
}
