using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace RealEstate.Mvc.Controllers
{
    public class HomeController : Controller
    {
        public static RealEstateContext Context = new RealEstateContext();
        public async Task<IActionResult> Index()
        {
            var buildInfoCommand = new BsonDocument("buildinfo", 1);
            var buildInfo = await Context.Database.RunCommandAsync<BsonDocument>(buildInfoCommand);
            return Content(buildInfo.ToJson(), "application/json");
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
