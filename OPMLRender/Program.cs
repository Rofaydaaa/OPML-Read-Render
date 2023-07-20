using OPMLRender.Pages;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapPost("/toggle-fav", async (HttpContext context) =>
{
    var requestForm = await context.Request.ReadFormAsync();
    var link = requestForm["link"];
    var title = requestForm["title"];

    var favoriteFeedsJson = context.Request.Cookies["StarFeeds"];
    var favoriteFeeds = string.IsNullOrEmpty(favoriteFeedsJson)
        ? new List<FeedDetails>()
        : JsonSerializer.Deserialize<List<FeedDetails>>(favoriteFeedsJson);

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
    context.Response.Cookies.Append("StarFeeds", serializedFavoriteFeeds);

    // Return the updated favorite status as part of the JSON response
    context.Response.StatusCode = StatusCodes.Status200OK;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync(JsonSerializer.Serialize(new { IsFavorite = feed.IsFavorite }));
});

app.MapRazorPages();

app.Run();
