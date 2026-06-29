namespace GutendexApp.Models;

//podaci o autoru
public record Author(string Name, int? BirthYear, int? DeathYear);
// podaci za jednu knjigu
public record Book(int Id, string Title, List<Author> Authors, string Summaries);

public record ApiResponse(int Count, string? Next, string? Previous, List<Book>? Results);


//poruke
public record FetchBooksByAuthor(string AuthorName); //ovo ide ka akteru    
public record TopicResult(string Author, List<string> Topics); // Odgovor od aktera
public record AddBookData(Book Book);