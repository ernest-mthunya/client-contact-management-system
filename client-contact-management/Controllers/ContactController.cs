using Microsoft.AspNetCore.Mvc;

namespace client_contact_management.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
