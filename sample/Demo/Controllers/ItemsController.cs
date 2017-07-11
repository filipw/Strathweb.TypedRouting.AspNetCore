using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Controllers
{
    public class ItemsController : Controller
    {
        public IEnumerable<Item> Get()
        {
            return new Item[] { new Item { Text = "value1" }, new Item { Text = "value2" } };
        }

        public Item Get(int id)
        {
            return new Item { Text = "value" };
        }

        public IActionResult Post([FromBody]Item item)
        {
            // one way to generate a link
            var link = Url.Link("GetItemById", new { id = 1 });

            // another way to geenrate a link, using the built in action results
            var result = CreatedAtRoute("GetItemById", new { id = 1 }, item);
            return result;
        }

        public Item Put(int id, [FromBody]Item item)
        {
            return item;
        }

        public int Delete(int id)
        {
            return id;
        }
    }
}
