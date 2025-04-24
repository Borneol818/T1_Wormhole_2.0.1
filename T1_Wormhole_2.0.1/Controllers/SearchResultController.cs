using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using T1_Wormhole_2._0._1.Models.Database;
using T1_Wormhole_2._0._1.Models.DTOs;

namespace T1_Wormhole_2._0._1.Controllers
{
    public class SearchResultController : Controller
    {      
        
        public IActionResult ShowResult()
        {            
            return View();
        }
       
    }
}
