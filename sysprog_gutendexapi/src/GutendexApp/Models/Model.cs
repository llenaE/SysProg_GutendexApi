using Akka.Actor;
using System.Text.Json.Serialization;
namespace GutendexApp.Models;

//podaci o autoru
//public record Author(string Name, int? BirthYear, int? DeathYear);
public record Author(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("birth_year")] int? BirthYear,
    [property: JsonPropertyName("death_year")] int? DeathYear
);
// podaci za jednu knjigu
//public record Book(int Id, string Title, List<Author> Authors, List<string> Summaries);
public record Book(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("authors")] List<Author> Authors,
    [property: JsonPropertyName("summaries")] List<string> Summaries
);

// public record ApiResponse(int Count, string? Next, string? Previous, List<Book>? Results);
public record ApiResponse(
    [property: JsonPropertyName("count")] int Count,
    [property: JsonPropertyName("next")] string? Next,
    [property: JsonPropertyName("previous")] string? Previous,
    [property: JsonPropertyName("results")] List<Book>? Results
);


//poruke
public record FetchBooksByAuthor(string AuthorName); //ovo ide ka akteru    
public record TopicResult(string Author, List<string> Topics); // Odgovor od aktera
public record AddBookData(Book Book);


// public record BookSummaryReceived(string Summary);//rxservice da ne salje ceo odgovor vec sao
  public record Passivate(IActorRef ActorRef);

 public record BookFound(Book Book);
public record BooksCompleted;
public record BooksFailed(string Error);