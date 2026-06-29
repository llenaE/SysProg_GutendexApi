using System;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text.Json;
using Akka.Pattern;
using GutendexApp.Models;

namespace GutendexApp.Services;

public class RxService
{
    private readonly HttpClient _httpClient;

    public RxService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    
    }
    public IObservable<Book> StreamBooks(string author)
{
    var firstUrl = $"https://gutendex.com/books?search={Uri.EscapeDataString(author)}";

    return Observable.FromAsync(() => FetchPageAsync(firstUrl))
        .Expand(apiResponse => 
            // ovako prolazimo koroz sve stranice, jer se sve knjige ne nalaze na jedno strani
            string.IsNullOrEmpty(apiResponse?.Next) 
                ? Observable.Empty<ApiResponse>() 
                : Observable.FromAsync(() => FetchPageAsync(apiResponse.Next)))
        .SelectMany(apiResponse => apiResponse?.Results ?? new List<Book>())
        .Catch<Book, Exception>(ex => 
        {
            
            Console.WriteLine($"[RxService Critical Error]: {ex.Message}");
            return Observable.Empty<Book>(); 
        });
}

//metoda kojoj se prosledjuje ur, jer je moguce da imamo vise stranica gde ce biti sve knjige a za svaku stranicu je potreban drugi url
private async Task<ApiResponse> FetchPageAsync(string url)
{
    var response = await _httpClient.GetAsync(url);
    response.EnsureSuccessStatusCode();
    var json = await response.Content.ReadAsStringAsync();


    var result= JsonSerializer.Deserialize<ApiResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    if(result==null)
       throw new Exception("Deserialization failed");
       return result;
}

}

