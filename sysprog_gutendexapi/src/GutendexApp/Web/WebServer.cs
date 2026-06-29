using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Akka.Actor;
using GutendexApp.Actors;
using GutendexApp.Models;

namespace GutendexApp.Web;


public class Server
{

    //!!! null su jer me nervirao warning, tek treba da ih iskoristimo
    private ActorSystem _actorSystem = null!;

    private IActorRef _authCoordinator = null!;
    private HttpListener _listener = new();
    private bool _isRunning = true;
    public async Task StartAsync()
    {

        _actorSystem = ActorSystem.Create("BookAnalysisSystem");
        //kreiramo kordinatora 
        _authCoordinator = _actorSystem.ActorOf(Props.Create<CoordinatorActor>(), "coordinator");

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
    private async Task HandleRequest(HttpListenerContext context)
    {

        var request = context.Request;
        var response = context.Response;

        var path = request.Url?.AbsolutePath.Trim('/');

        Console.WriteLine($"Request: {path}");

        if (path == "favicon.ico")
        {
            context.Response.StatusCode = 204;
            context.Response.Close();
            //obrisala sam task.completedtask jer mi je pravilo problem ali mogle bi da pitamo 
            // jel mora ovo uopste da bude async
            return;
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            RespondWithText(context, "Nedostaje ime autora", 400);
            return;
        }


        // var author = path;

        // ovde da se odradi slanje RxService-u
        //valjda saljemo aktorima ? 
        try
        {

            var requestMsg = new FetchBooksByAuthor(path);

            //saljemo koordinatoru
            //cekamo (asinhrono) TopicResult odgovor od AuthorActor-a.
            //cekamo maks 5s 
            var result = await _authCoordinator.Ask<TopicResult>(requestMsg, TimeSpan.FromSeconds(5));

            //vracamo klijentu odgovor
            RespondWithJson(context, result);
        }
        catch (TaskCanceledException)
        {
            RespondWithText(context, "Greška: Isteklo vreme čekanja (Timeout). Aktor je preopterećen.", 504);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Greška pri obradi: {ex.Message}");
            RespondWithText(context, "Greška na serveru: " + ex.Message, 500);
        }
        finally
        {
            //realno ove respond metode nam vec zatvaraju stream ne znam da li je ovde potrebno
            if (context.Response.OutputStream.CanWrite)
            {
                context.Response.OutputStream.Close();
            }
        }

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
            byte[] buffer = JsonSerializer.SerializeToUtf8Bytes(content, new JsonSerializerOptions { WriteIndented = true });

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