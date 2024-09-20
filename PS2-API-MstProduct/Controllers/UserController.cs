using Microsoft.AspNetCore.Mvc;

namespace PS2_API_MstProduct.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
