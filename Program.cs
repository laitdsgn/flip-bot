var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

List<Offer> offers = new();
offers.Add(new Offer("dadsa", 2, "dasdasasd")); // <- przed app.Run()

app.MapGet("/offers", () => Results.Json(offers));

app.MapGet("/", () => "Hello World!");

app.Run();

record Offer(string Title, decimal Price, string Link);