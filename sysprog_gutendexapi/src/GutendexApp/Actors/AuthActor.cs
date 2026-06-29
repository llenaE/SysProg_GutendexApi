using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using Akka.Actor;
using Akka.Event;
using GutendexApp.Actors;
using GutendexApp.Models;
using GutendexApp.Services;

namespace GutendexApp.Actors;


public class AuthorActor : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly string _authorName;
    private List<Book> _books = new();
    private readonly TopicModelingService _mlService = new();
    public AuthorActor(string authorName)
    {
        _authorName = authorName;
        //samo da vidim da li radi bar nesto
        _books.Add(new Book(1, "Pride and Prejudice", new List<Author>(), "A story about love, marriage, and social class in 19th century England. Elizabeth Bennet deals with issues of manners, upbringing, morality, education, and marriage in the society of the landed gentry."));
        _books.Add(new Book(2, "Emma", new List<Author>(), "Emma Woodhouse is a clever, rich, and spoiled young woman who meddles in the romantic lives of others. It explores themes of social hierarchy, romance, and the folly of youth."));
        _books.Add(new Book(3, "War and Peace", new List<Author>(), "An epic tale of Russian society during the Napoleonic era. It explores philosophy, history, and the human condition during times of war and peace."));
        _books.Add(new Book(4, "The Great Gatsby", new List<Author>(), "A story of wealth, love, and the American dream in the roaring twenties. It focuses on social status and the pursuit of happiness."));
        _books.Add(new Book(5, "1984", new List<Author>(), "A dystopian social science fiction novel and cautionary tale about the dangers of totalitarianism, mass surveillance, and repressive regimentation of persons and behaviors within society."));
        //ovde cemo pozivati rxService
        // var rxClient = new RxService();
        // rxClient.StreamBooks(_authorName).Subscribe(b => Self.Tell(new BookFound(b)));

        Receive<FetchBooksByAuthor>(request =>
        {
            _log.Info($"[ML] Pokrećem analizu za autora: {_authorName} na {_books.Count} knjiga.");

            if (_books.Count == 0)
            {
                Sender.Tell(new TopicResult(_authorName, new List<string> { "Nema dovoljno podataka za analizu." }));
                return;
            }

            try
            {
                //pozivamo ml 
                var topics = _mlService.Analyze(_books);
                Sender.Tell(new TopicResult(_authorName, topics));
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"Greška tokom ML analize za {_authorName}");
                Sender.Tell(new TopicResult(_authorName, new List<string> { "Greška pri analizi." }));
            }
        });
    }

    protected override void PreStart() => _log.Info($"AuthorActor za {_authorName} je pokrenut.");
}