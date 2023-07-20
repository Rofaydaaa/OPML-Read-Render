using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System;
using System.Xml;
using System.Xml.Linq;
using System.Text.Json;

namespace OPMLRender.Pages
{
    public class FavoritesModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        public int PageSize { get; set; } = 10;
        public List<FeedDetails> FavoriteFeeds { get; set; } = new List<FeedDetails>();

        public FavoritesModel(ILogger<IndexModel> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGet([FromQuery] int page = 1)
        {
            FavoriteFeeds = JsonSerializer.Deserialize<List<FeedDetails>>(Request.Cookies["StarFeeds"]);
            var itemCount = FavoriteFeeds.Count;

            var startIndex = (page - 1) * PageSize;
            var endIndex = startIndex + PageSize;

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)itemCount / PageSize);

            return Page();
        }
    }
}
