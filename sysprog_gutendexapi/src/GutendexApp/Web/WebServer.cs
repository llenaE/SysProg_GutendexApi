using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Akka.Actor;

namespace GutendexApp.Web;


public class Server
{

    //!!! null su jer me nervirao warning, tek treba da ih iskoristimo
    private ActorSystem _actorSystem=null!;

    private IActorRef _bookCoordinator=null!;
    private HttpListener _listener=new();
    private bool _isRunning = true;
   public async Task StartAsync()
    {
       
        _actorSystem = ActorSystem.Create("BookAnalysisSystem");
      
       
        _listener = new HttpListener();
        _listener.Prefixes.Add("http://localhost:5000/");
        _listener.Start();
        Console.WriteLine("Server sluša na adresi: http://localhost:5000/");
        Console.WriteLine("Pritisni 'Q' za gašenje servera.");

         _ = Task.Run(() => ListenForShutdown());

        while (_isRunning)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => HandleRequest(context)); 
            }
            catch (HttpListenerException) when (!_isRunning)
            {
                
            }
        }
    }
private void ListenForShutdown()
{
    while (_isRunning)
    {
        if (Console.ReadKey(true).Key == ConsoleKey.Q)
            {
                Console.WriteLine("\nGašenje servera...");
                StopAsync().GetAwaiter().GetResult();
                break;
            }
    }
}
    private  Task HandleRequest(HttpListenerContext context)
    {
     
    var request = context.Request;
    var response =context.Response;

    var path = request.Url?.AbsolutePath.Trim('/');

    Console.WriteLine($"Request: {path}");

    if (path == "favicon.ico")
    {
        context.Response.StatusCode = 204; 
        context.Response.Close();
        return Task.CompletedTask;
    }

    if (string.IsNullOrWhiteSpace(path))
    {
        RespondWithText(context, "Nedostaje ima autora", 400);
        return Task.CompletedTask;
    }

    
    var author = path;

    // ovde da se odradi slanje RxService-u
    Console.WriteLine($"Autor: {author}");

    var result = new
    {
        Author = author,
        Message = "Author uspesno stigao"
    };

    RespondWithJson(context, result);

    return Task.CompletedTask;
    
    }
       

    public async Task StopAsync()
    {
        Console.WriteLine("\n[SHUTDOWN] Pokrećem Graceful Shutdown...");

        
        _isRunning = false;
        _listener?.Stop();

        if (_actorSystem != null)
        {
    
            await _actorSystem.Terminate();
        }
        
        Console.WriteLine("[SHUTDOWN] Akka.NET sistem i HTTP server su uspešno ugašeni.");
    }


private void RespondWithJson(HttpListenerContext context, object content, int statusCode = 200)
{
    try
    {
        var response = context.Response;
        byte[] buffer = JsonSerializer.SerializeToUtf8Bytes(content,new JsonSerializerOptions { WriteIndented = true } );

        response.StatusCode = statusCode;
        response.ContentType = "application/json";
        response.ContentLength64 = buffer.Length;

        response.OutputStream.Write(buffer, 0, buffer.Length);
    }
    finally
    {
        context.Response.OutputStream.Close();
        context.Response.Close();
    }
}


private void RespondWithText(HttpListenerContext context, string text, int statusCode = 200)
{
    try
    {
        var response = context.Response;

        byte[] buffer = Encoding.UTF8.GetBytes(text);
        response.StatusCode = statusCode;
        response.ContentType = "text/plain";
        response.ContentLength64 = buffer.Length;

        response.OutputStream.Write(buffer, 0, buffer.Length);
    }
    finally
    {
        context.Response.OutputStream.Close();
        context.Response.Close();
    }
}
}