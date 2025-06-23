using System.Runtime.CompilerServices;
using HtmlAgilityPack;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello world");

const int PHONE_PRICE = 1000;

List<Offer> offers = new();
List<HtmlNode> productNodes = new List<HtmlNode>();

app.MapGet("/offers", () => Results.Json(offers));

app.Lifetime.ApplicationStarted.Register(() =>
{
    _ = Task.Run(async () =>
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

        try
        {
            var html = await client.GetStringAsync(
                $"https://www.olx.pl/elektronika/telefony/smartfony-telefony-komorkowe/iphone/?courier=1&search%5Bphotos%5D=1&search%5Border%5D=created_at:desc&search%5Bfilter_float_price:to%5D={PHONE_PRICE}&search%5Bfilter_enum_phonemodel%5D%5B0%5D=iphone-16&search%5Bfilter_enum_phonemodel%5D%5B1%5D=iphone-14-pro-max&search%5Bfilter_enum_phonemodel%5D%5B2%5D=iphone-13-pro&search%5Bfilter_enum_phonemodel%5D%5B3%5D=iphone-13&search%5Bfilter_enum_phonemodel%5D%5B4%5D=iphone-12&search%5Bfilter_enum_phonemodel%5D%5B5%5D=iphone-14-pro&search%5Bfilter_enum_phonemodel%5D%5B6%5D=iphone-14&search%5Bfilter_enum_phonemodel%5D%5B7%5D=iphone-15&search%5Bfilter_enum_phonemodel%5D%5B8%5D=iphone-16-plus&search%5Bfilter_enum_phonemodel%5D%5B9%5D=iphone-16pro&search%5Bfilter_enum_phonemodel%5D%5B10%5D=iphone-11-pro&search%5Bfilter_enum_phonemodel%5D%5B11%5D=iphone-16-pro-max&search%5Bfilter_enum_phonemodel%5D%5B12%5D=iphone-11-pro-max&search%5Bfilter_enum_phonemodel%5D%5B13%5D=iphone-15-pro-max&search%5Bfilter_enum_phonemodel%5D%5B14%5D=iphone-15-plus&search%5Bfilter_enum_phonemodel%5D%5B15%5D=iphone-15-pro&search%5Bfilter_enum_phonemodel%5D%5B16%5D=iphone-14-plus&search%5Bfilter_enum_phonemodel%5D%5B17%5D=iphone-12-pro-max&search%5Bfilter_enum_phonemodel%5D%5B18%5D=iphone-12-pro&search%5Bfilter_enum_phonemodel%5D%5B19%5D=iphone-13-pro-max"
            );
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            foreach (var node in doc.DocumentNode.SelectNodes("//div[@data-cy='l-card']") ?? null)
            {
                Console.WriteLine(node);

                var offerTitle =
                    node.SelectSingleNode(".//div[@data-cy='ad-card-title']/a/h4")?.InnerText
                    ?? "Brak tytu³u";
                var offerPrice =
                    node.SelectSingleNode(".//p[@data-testid='ad-price']")?.InnerText
                    ?? "Brak ceny";
                var offerUrl = node.SelectSingleNode(".//div[@data-cy='ad-card-title']/a")
                    ?.GetAttributeValue("href", "Brak linku");

                offers.Add(new Offer(offerTitle, offerPrice, offerUrl));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing HTML: {ex.Message}");
        }
    });
});

app.Run();
