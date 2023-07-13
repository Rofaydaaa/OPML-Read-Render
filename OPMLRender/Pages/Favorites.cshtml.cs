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

        public async Task<IActionResult> OnPostToggleFavorite(string link, int page)
        {
            try
            {
                var favoriteFeeds = JsonSerializer.Deserialize<List<FeedDetails>>(Request.Cookies["StarFeeds"]);
                var favoriteFeed = favoriteFeeds.FirstOrDefault(f => f.Link == link);
                var feed = favoriteFeeds.FirstOrDefault(f => f.Link == link);
                if (feed != null)
                {
                    favoriteFeeds.Remove(feed);
                    feed.IsFavorite = false;
                }
                else
                {
                    feed = new FeedDetails { Link = link, IsFavorite = true };
                    favoriteFeeds.Add(feed);
                }
                var serializedFavoriteFeeds = JsonSerializer.Serialize(favoriteFeeds);
                Response.Cookies.Append("StarFeeds", serializedFavoriteFeeds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while toggling favorite status.");
            }

            return RedirectToPage();
        }
    }
}