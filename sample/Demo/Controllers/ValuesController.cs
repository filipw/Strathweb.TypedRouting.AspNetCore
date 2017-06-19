using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Controllers
{
    public class ValuesController : Controller
    {
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        public string Get(int id)
        {
            return "value";
        }

        public IActionResult Post([FromBody]string value)
        {
            // one way to generate a link
            var link = Url.Link("GetValueById", new { id = 1 });

            // another way to geenrate a link, using the built in action results
            var result = CreatedAtRoute("GetValueById", new { id = 1 }, "foo");
            return result;
        }

        public void Put(int id, [FromBody]string value)
        {
        }

        public void Delete(int id)
        {
        }
    }
}
