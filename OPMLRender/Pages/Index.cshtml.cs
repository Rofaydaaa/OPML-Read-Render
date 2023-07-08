using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

                    foreach (XmlNode itemNode in paginatedItemNodes)
                    {
                        var itemDetails = new FeedDetails();
                        itemDetails.Title = itemNode.SelectSingleNode("@text")?.Value;
                        itemDetails.Link = itemNode.SelectSingleNode("@xmlUrl")?.Value;
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
    }

    public class FeedDetails
    {
        public string Title { get; set; }
        public string Link { get; set; }
    }
}