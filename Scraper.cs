using System.Data.SqlTypes;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using HtmlAgilityPack;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello world");

const int DEVICE_PRICE = 500;
List<string> BLACK_LIST_WORD = new List<string>
{
    "etui",
    "obudowa",
    "szk³o",
    "szklo",
    "folia",
    "hydro¿el",
    "case",
    "niesprawny",
    "pude³ko",
    "pudelko",
    "na czêœci",
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

        while (true)
        {
            try
            {
                var html = await client.GetStringAsync(
                    $"{Urls.Iphones}&search%5Bfilter_float_price:to%5D={DEVICE_PRICE}"
                );

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                foreach (var node in doc.DocumentNode.SelectNodes("//div[@data-cy='l-card']"))
                {
                    var offerTitle =
                        node.SelectSingleNode(".//div[@data-cy='ad-card-title']/a/h4")?.InnerText
                        ?? "Brak tytu³u";
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
                        Console.WriteLine("invalid offer");
                        rejectedOffers.Add(offerUrl); // Add offer to rejected offers list so it doesent get validated again
                    }
                    else
                    {
                        offers.Add(new Offer(offerTitle, offerPrice, offerUrl, offerDate));
                    }
                }
                Console.WriteLine($"minutesCounter = {minutesCounter}");
                minutesCounter += 0.5;

                if (minutesCounter >= 15)
                {
                    offers.Clear();
                    Console.WriteLine("Offers list cleared after 15 minutes.");
                    minutesCounter = 0;
                    offers = offers.GroupBy(o => o.Link).Select(g => g.First()).ToList();
                }

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing HTML: {ex.Message}");
            }
            string json = JsonSerializer.Serialize(offers);
            File.WriteAllText("backup.json", json);
            await Task.Delay(TimeSpan.FromMinutes(0.5));
            File.WriteAllText("backup.json", "");
        }
    });
});

app.Run();
