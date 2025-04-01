using System;

namespace Price_Scraping;
class Program
{
    public static async Task Main(string[] args)
    {
        Console.Write("Paste Amazon Link -> ");
        string url = Console.ReadLine();
        // Console.WriteLine(url);
        if (!IsValidUrl(url))
        {
            Console.WriteLine("Link is invalid");
        }
    }
    
    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out Uri? result) && (result.Scheme == Uri.UriSchemeHttps);
    }
}