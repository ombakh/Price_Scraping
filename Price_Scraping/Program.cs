using System;

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
    }
    
    private static bool IsValidAmazonUrl(string url) // parses url to ensure validity
    {
        // checks that the url is valid in the first place
        bool validUrl = Uri.TryCreate(url, UriKind.Absolute, out Uri? result) && (result.Scheme == Uri.UriSchemeHttps);
        // checks for "amazon.com" in url
        bool validAmazon = validUrl && result.Host.Contains("amazon.com", StringComparison.OrdinalIgnoreCase);
        return validUrl && validAmazon; // returns both values to ensure both are true
    }
}