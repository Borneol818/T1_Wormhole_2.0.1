using Microsoft.AspNetCore.Mvc;

namespace T1_Wormhole_2._0._1.Controllers
{
    public class NewsArticleController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }




        public IActionResult NewsArticle()
        {
            return View();
        }

        public IActionResult newsPoster()
        {
            return View();
        }
    }
}
