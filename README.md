# 📌 FLIP-BOT

**flip-bot** is a simple tool for finding great deals on **OLX.pl** (smartphones and PC parts).  
It consists of:  

✅ **ASP.NET Core API** (with HtmlAgilityPack for scraping offers)  
✅ **Frontend (HTML5 + JavaScript)** to display offers in your browser  
✅ **Optional Discord Bot** that sends new deals directly to your Discord channel  
✅ **Cloudflare Tunnel support** for easy public hosting

The bot comes with **3 preset categories** (iPhones, Samsungs, and CPUs), but you can easily add your own custom URLs.

---

## 🚀 Features

- Scrapes OLX offers in real time  
- Filters out blacklisted keywords (e.g., “case”, “broken”, “screen”)  
- Automatically removes old offers (older than 2 hours)  
- Shows time since the offer was added (e.g., “5m ago” or “1h 20m ago”)  
- Saves data to `backup.json`  
- Discord bot support with automatic deal notifications  

---

## 🛠️ Technologies Used

- **Backend:** ASP.NET Core 9 (C#) + HtmlAgilityPack  
- **Frontend:** HTML5, CSS, JavaScript  
- **Hosting (optional):** Cloudflare Tunnel  
- **Bot:** Discord.js (Node.js)  

---

## 📥 Installation & Setup

### ✅ 1. Clone the repository

```bash
git clone https://github.com/laitdsgn/flip-bot.git
cd flip-bot
```
### ✅ 2. Install requirements

### Windows:

- Download .NET9 SDK
- Open a terminal in the project folder and run:

```bash
dotnet build
dotnet run
```

### ✅ 3. Run the frontend
Simply open index.html in your browser.


