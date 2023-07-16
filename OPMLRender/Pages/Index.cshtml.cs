using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Xml;

namespace OPMLRender.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        public int PageSize { get; set; } = 10;
        public List<FeedDetails> FeedsDetails { get; set; } = new List<FeedDetails>();

        public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGet([FromQuery] int page = 1)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync("https://blue.feedland.org/opml?screenname=dave");
                XmlDocument xmlDoc = new XmlDocument();
                if (!Request.Cookies.ContainsKey("StarFeeds"))
                {
                    var serializedFavoriteFeeds = JsonSerializer.Serialize(new List<FeedDetails>());
                    Response.Cookies.Append("StarFeeds", serializedFavoriteFeeds);
                }

                if (response.IsSuccessStatusCode)
                {
                    var xmlContent = await response.Content.ReadAsStringAsync();
                    xmlDoc.LoadXml(xmlContent);
                    var itemNodes = xmlDoc.SelectNodes("opml/body/outline");
                    var itemCount = itemNodes.Count;

                    var startIndex = (page - 1) * PageSize;
                    var endIndex = startIndex + PageSize;
                    var paginatedItemNodes = itemNodes.Cast<XmlNode>()
                        .Skip(startIndex)
                        .Take(PageSize);

                    var favoriteFeeds = JsonSerializer.Deserialize<List<FeedDetails>>(Request.Cookies["StarFeeds"]);
                    foreach (XmlNode itemNode in paginatedItemNodes)
                    {
                        var itemDetails = new FeedDetails();
                        itemDetails.Title = itemNode.SelectSingleNode("@text")?.Value;
                        itemDetails.Link = itemNode.SelectSingleNode("@xmlUrl")?.Value;

                        var favoriteFeed = favoriteFeeds.FirstOrDefault(f => f.Link == itemDetails.Link);
                        if (favoriteFeed != null)
                        {
                            itemDetails.IsFavorite = true;
                        }
                        FeedsDetails.Add(itemDetails);
                    }
                    ViewData["CurrentPage"] = page;
                    ViewData["TotalPages"] = (int)Math.Ceiling((double)itemCount / PageSize);

                    return Page();
                }
                else
                {
                    _logger.LogError("Unsuccessful status code");
                    return RedirectToPage("/Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the request.");
                return RedirectToPage("/Error");
            }
        }

        public async Task<IActionResult> OnPostToggleFavorite(string link, string title, int page)
        {
            try
            {
                var favoriteFeeds = JsonSerializer.Deserialize<List<FeedDetails>>(Request.Cookies["StarFeeds"]);
                var feed = favoriteFeeds.FirstOrDefault(f => f.Link == link);
                if (feed != null)
                {
                    favoriteFeeds.Remove(feed);
                    feed.IsFavorite = false;
                }
                else
                {
                    feed = new FeedDetails { Link = link, Title = title, IsFavorite = true };
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

    public class FeedDetails
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public bool IsFavorite { get; set; } = false;
    }
}
