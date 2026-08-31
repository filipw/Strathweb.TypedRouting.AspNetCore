using Microsoft.AspNetCore.Mvc;

namespace Demo
{
    // minimal API handlers written as method groups, so they can be referenced
    // from an expression and used as a link generation target
    public static class MinimalHandlers
    {
        // deliberately never mapped to an endpoint
        public static string Unmapped(int id) => $"unmapped {id}";

        public static Item GetItem(int id) => new Item { Text = $"minimal {id}" };

        public static string Search(string q, int page) => $"{q}/{page}";

        public static async Task<string> GetAsync(int id)
        {
            await Task.Delay(1);
            return $"async {id}";
        }

        // the posted item and the injected service can never appear in a URL
        public static string Create(int tenantId, [FromBody] Item item, [FromServices] TimerFilter timer) => $"{tenantId}:{item.Text}";
    }
}
