using Microsoft.AspNetCore.Mvc;

namespace T1_Wormhole_2._0._1.Controllers
{
    public class informationPageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Poster()
        {
            return View();
        }
    }
}
