using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Encodings.Web;
using System.Xml;

namespace OPMLRender.Pages
{
    public class XMLParserModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private string? _link { get; set; }
        public List<ItemDetails> ItemsDetails { get; set; } = new List<ItemDetails>();

        public XMLParserModel(ILogger<IndexModel> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGet()
        {
            try
            {
                if (!Request.Query.ContainsKey("link"))
                {
                    _logger.LogError("Feed link is not found");
                    return RedirectToPage("/Error");
                }
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync(Request.Query["link"]);
                XmlDocument xmlDoc = new XmlDocument();

                if (response.IsSuccessStatusCode)
                {
                    var xmlContent = await response.Content.ReadAsStringAsync();
                    xmlDoc.LoadXml(xmlContent);
                    foreach (XmlNode itemNode in xmlDoc.SelectNodes("rss/channel/item"))
                    {
                        var itemDetails = new ItemDetails();
                        itemDetails.Description = itemNode.SelectSingleNode("description")?.InnerText;
                        itemDetails.PubDate = itemNode.SelectSingleNode("pubDate")?.InnerText;
                        itemDetails.Link = itemNode.SelectSingleNode("link")?.InnerText;
                        itemDetails.Guide = itemNode.SelectSingleNode("guid")?.InnerText;
                        ItemsDetails.Add(itemDetails);
                    }
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

    public class ItemDetails
    {
        public string Description { get; set; }
        public string PubDate { get; set; }
        public string Link { get; set; }
        public string Guide { get; set; }
    }
}

