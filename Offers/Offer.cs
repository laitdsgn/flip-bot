using System.Globalization;

class Offer
{
    public string Title { get; set; }
    public string Price { get; set; }
    public string Link { get; set; }

    public string Date { get; set; }

    public Offer(string title, string price, string link, string date)
    {
        Title = title;
        Price = price;
        Link = link;
        Date = date;
    }
}
