using Microsoft.AspNetCore.Mvc;

namespace Demo.Controllers
{
    // a plain attribute routed controller - no typed route is registered for it,
    // to show that typed link generation works in any MVC app
    [ApiController]
    [Route("plain")]
    public class PlainController : ControllerBase
    {
        [HttpGet("{id}", Name = "PlainById")]
        public string ById(int id) => id.ToString();

        [HttpGet("unnamed/{id}")]
        public string Unnamed(int id) => id.ToString();
    }
}
