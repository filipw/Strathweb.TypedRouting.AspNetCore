using Microsoft.AspNetCore.Mvc;

namespace Demo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/reports")]
    public class ReportsController : Controller
    {
        [HttpGet("{id}")]
        public string Get(int id) => $"report {id}";
    }
}
