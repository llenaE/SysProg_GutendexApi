using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Net.Http.Json;
using GutendexApp.Models;

namespace GutendexApp.Services;

//klasa koja ce da komunicira sa API_jem
public class RxService
{
    private readonly HttpClient _httpClient;

    public RxService(HttpClient httpClient)=>_httpClient=httpClient;

    // public async Task<List<Book>> FetchBooks(string author)
    // {
      
    // }

    
}
