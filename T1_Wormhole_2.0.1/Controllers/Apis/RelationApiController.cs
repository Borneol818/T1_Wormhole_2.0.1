using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using T1_Wormhole_2._0._1.Models.Database;

namespace T1_Wormhole_2._0._1.Controllers.Apis
{
    [Route("api/[controller]")]
    [ApiController]
    public class RelationApiController : ControllerBase
    {
        private readonly WormHoleContext _db;

        public RelationApiController(WormHoleContext db)
        {
            _db = db;
        }

        

    }
}
