using Microsoft.AspNetCore.Mvc;
using Strathweb.TypedRouting.AspNetCore;

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

        public IActionResult Post([FromBody] Item item)
        {
            // typed link generation - the action is referenced directly, so a rename is a compile error.
            // the [FromBody] parameter of this action is never part of a URL, so it is left out automatically
            var link = Url.Link<ItemsController>(x => x.Get(1));
            Response.Headers.Append("TypedLink", link);

            return this.CreatedAtAction<ItemsController>(x => x.Get(1), item);
        }

        public Item Put(int id, [FromBody] Item item)
        {
            return item;
        }

        public int Delete(int id)
        {
            return id;
        }
    }
}
