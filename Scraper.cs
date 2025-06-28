using System.Data.SqlTypes;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using HtmlAgilityPack;

var builder = WebApplication.CreateBuilder(args);

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors();

app.MapGet("/", () => "Hello world");

const int DEVICE_PRICE = 500;
List<string> BLACK_LIST_WORD = new List<string>
{
    "etui",
    "obudowa",
    "szkło",
    "szklo",
    "folia",
    "hydrożel",
    "case",
    "niesprawny",
    "pudełko",
    "pudelko",
    "na części",
    "na czesci",
    "simlock",
    "zablokowany",
    "blokada",
    "ekran",
    "kabel",
    "przewód",
    "kamerka",
    "bateria",
    "akumulator",
    "erui",
};

double minutesCounter = 0;

List<Offer> offers = new();
HashSet<string> rejectedOffers = new HashSet<string>();
List<HtmlNode> productNodes = new List<HtmlNode>();

app.MapGet("/offers", () => Results.Json(offers));

app.Lifetime.ApplicationStarted.Register(() =>
{
    _ = Task.Run(async () =>
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

        client.DefaultRequestHeaders.Add(
            "Accept",
            "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8"
        );
        client.DefaultRequestHeaders.Add("DNT", "1");
        client.DefaultRequestHeaders.Add("Connection", "keep-alive");
        client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");

        while (true)
        {
            try
            {
                var html = await client.GetStringAsync(
                    $"{Urls.Iphones}&search%5Bfilter_float_price:to%5D={DEVICE_PRICE}"
                );

                var doc = new HtmlDocument();

                doc.LoadHtml(html);

                // delete offers older than 2 hours
                offers = offers
                    .Where(o => (DateTime.Now - o.DateAdded) <= TimeSpan.FromHours(2))
                    .ToList();

                foreach (var node in doc.DocumentNode.SelectNodes("//div[@data-cy='l-card']"))
                {
                    var offerTitle =
                        node.SelectSingleNode(".//div[@data-cy='ad-card-title']/a/h4")?.InnerText
                        ?? "Brak tytu�u";
                    var offerPrice =
                        node.SelectSingleNode(".//p[@data-testid='ad-price']")?.InnerText
                        ?? "Brak ceny";
                    var offerUrl =
                        "https://www.olx.pl"
                        + node.SelectSingleNode(".//div[@data-cy='ad-card-title']/a")
                            ?.GetAttributeValue("href", "Brak linku");
                    var offerDate =
                        node.SelectSingleNode(".//p[@data-testid='location-date']")?.InnerText
                        ?? "Brak daty";

                    var offerImg =
                        node.SelectSingleNode(".//div/img")?.GetAttributeValue("src", null) ?? null;

                    // skip un loaded
                    if (offerImg.Contains("no_thumbnail") || offerImg == null)
                    {
                        offerImg = null;
                    }
                    else if (offerImg.Contains("olxcdn.com"))
                    {
                        // replace small img dimensions with larger
                        offerImg = System.Text.RegularExpressions.Regex.Replace(
                            offerImg,
                            @";s=\d+x\d+;",
                            ";s=1000x2000;"
                        );
                    }

                    offerDate = offerDate.Substring(offerDate.LastIndexOf("-") + 2);

                    if (offerPrice.Contains("do negocjacji"))
                    {
                        offerPrice = offerPrice.Substring(0, (offerPrice.IndexOf("do negocjacji")));
                    }

                    if (rejectedOffers.Contains(offerUrl))
                    {
                        Console.WriteLine("offer already rejected, skipping");
                        continue;
                    }

                    bool offerTitleContainsBlackListWord = BLACK_LIST_WORD.Any(word =>
                        offerTitle.Contains(word, StringComparison.OrdinalIgnoreCase)
                    );

                    if (
                        offerTitleContainsBlackListWord
                        || offers.Any(a => a.Link == offerUrl)
                        || !offerDate.Contains("Dzisiaj")
                    )
                    {
                        rejectedOffers.Add(offerUrl); // add offer to rejected offers list so it doesent get validated again
                    }
                    else
                    {
                        offers.Add(
                            new Offer(
                                offerTitle,
                                offerPrice,
                                offerUrl,
                                offerDate,
                                offerImg,
                                DateTime.Now
                            )
                        );
                    }
                }

                Console.WriteLine($"minutesCounter = {minutesCounter}");
                minutesCounter += 1;

                if (minutesCounter >= 60)
                {
                    Console.WriteLine("removed duplicates");
                    minutesCounter = 0;
                    offers = offers.GroupBy(o => o.Link).Select(g => g.First()).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing HTML: {ex.Message}");
            }

            string json = JsonSerializer.Serialize(offers);
            File.WriteAllText("backup.json", json);
            await Task.Delay(TimeSpan.FromMinutes(1));
            File.WriteAllText("backup.json", "");
        }
    });
});

app.UseDefaultFiles();

app.UseStaticFiles();

app.Run();
