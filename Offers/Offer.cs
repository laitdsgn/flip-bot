class Offer
{
    public string Title { get; set; }
    public string Price { get; set; }
    public string Link { get; set; }

    public Offer(string title, string price, string link)
    {
        Title = title;
        Price = price;
        Link = "olx.pl" + link;
    }
}
