using System.Threading.Tasks;
using GutendexApp;
using GutendexApp.Models;
using GutendexApp.Services;
using GutendexApp.Web;

namespace GutendexApp
{
    public class Program
    {
    static async Task Main(string[] args)
    {
        Console.WriteLine("===GUTENDEX SERVER STARTUP ===");
        try
        {

            Server server = new Server();
            await server.StartAsync();
           
        }
        catch (Exception ex)
        {
            Console.WriteLine($"KRITIČNA GREŠKA PRI POKRETANJU: {ex.Message}");

        }
        Console.WriteLine("Aplikacija je ugašena.");
    }
    }
}