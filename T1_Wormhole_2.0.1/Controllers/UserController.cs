using Microsoft.AspNetCore.Mvc;

namespace T1_Wormhole_2._0._1.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ViewProfile()
        {

            return View();
        }
        public IActionResult TestRelation()
        {

            return View();
        }
    }
}
