using System;
using Microsoft.Playwright;
using Microsoft.Data.Sqlite;

namespace Price_Scraping;
class Program
{
    public static async Task Main(string[] args)
    {
        Console.Write("Paste Amazon Link -> ");
        string url = Console.ReadLine();
        
        // checks to ensure url is not null
        if (string.IsNullOrWhiteSpace(url))
        {
            Console.WriteLine("Please enter a URL.");
            return;
        }
        
        // Console.WriteLine(url);
        if (!IsValidAmazonUrl(url))
        {
            Console.WriteLine("Not a valid Amazon URL");
        }
        else
        {
            Console.WriteLine("Valid Amazon URL detected; looking for price... ");
        }

        var (price, title) = await GetPrice(url);
        Console.WriteLine($"Price is {price}");
        
        Console.Write("What is your target price for this item -> $");
        string userTarget = Console.ReadLine();
        if (!float.TryParse(userTarget, out float targetPrice))
        {
            Console.WriteLine("Please enter a valid value.");
            return;
        }
        Console.WriteLine($"Got it, we will notify you when it reaches ${targetPrice}!");

        SavePriceData(title, price, targetPrice);
    }
    
    private static bool IsValidAmazonUrl(string url) // parses url to ensure validity
    {
        // checks that the url is valid in the first place
        bool validUrl = Uri.TryCreate(url, UriKind.Absolute, out Uri? result) && (result.Scheme == Uri.UriSchemeHttps);
        // checks for "amazon.com" in url
        bool validAmazon = validUrl && result.Host.Contains("amazon.com", StringComparison.OrdinalIgnoreCase);
        return validUrl && validAmazon; // returns both values to ensure both are true
    }

    private static async Task<(string price, string title)> GetPrice(string url)
    {
        try
        {
            using var playwright = await Playwright.CreateAsync(); // launches playwright...
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var page = await browser.NewPageAsync();
            
            await page.GotoAsync(url);
            
            var dollarLocator = page.Locator(".a-price-whole");
            var centsLocator = page.Locator(".a-price-fraction");
            var titleLocator = page.Locator("#productTitle");
            
            // gets text from async and assigns it to variables
            string dollars = await GetTextFromLocatorAsync(dollarLocator);
            string cents = await GetTextFromLocatorAsync(centsLocator);
            string title = await GetTextFromLocatorAsync(titleLocator);
            
            Console.WriteLine($"title: {title}");
            await browser.CloseAsync();

            if (!string.IsNullOrEmpty(dollars) && !string.IsNullOrEmpty(cents))
            {
                return ($"{dollars}{cents}", title);
            }

            return (string.Empty, title);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return (string.Empty, string.Empty);
        }
    }
    
    private static async Task<string> GetTextFromLocatorAsync(ILocator locator) // for cleaning up returned price
    {
        if (await locator.CountAsync() > 0)
        {
            string text = await locator.First.InnerTextAsync();
            return text.Trim().Replace("\n", "").Replace("\r", ""); // text returns with whitespace, this clears it up
        }
        return string.Empty;
    }

    private static void SavePriceData(string title, string price, float targetPrice)
    {
        string connectionString = "Data Source=priceData.db";

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            
            var createTableCommand = connection.CreateCommand();
            createTableCommand.CommandText = @"CREATE TABLE IF NOT EXISTS priceHistory (
                                                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                                    Title TEXT NOT NULL,
                                                    Price TEXT,
                                                    TargetPrice REAL NOT NULL,
                                                    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
                                                    );";
            createTableCommand.ExecuteNonQuery();

            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = @"INSERT INTO priceHistory (Title, Price, TargetPrice) 
                                          VALUES ($title, $price, $targetPrice)";
            insertCommand.Parameters.AddWithValue("$title", title);
            insertCommand.Parameters.AddWithValue("$price", price);
            insertCommand.Parameters.AddWithValue("$targetPrice", targetPrice);

            insertCommand.ExecuteNonQuery();
        }
    }
}