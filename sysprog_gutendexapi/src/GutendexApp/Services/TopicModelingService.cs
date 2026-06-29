using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Text;
using GutendexApp.Models;

namespace GutendexApp.Services;

public class BookTextData
{
    public string Summary { get; set; }
}

public class TopicModelingService
{
    private readonly MLContext _mlContext;

    public TopicModelingService()
    {
        _mlContext = new MLContext(seed: 1);
    }
    public List<string> Analyze(List<Book> books)
    {
        var validBooks = books.Where(b => !string.IsNullOrWhiteSpace(b.Summaries)).ToList();

        if (validBooks.Count < 2)
            return new List<string> { "Not enough data." };

        var data = validBooks.Select(b => new BookTextData { Summary = b.Summaries });
        var dataView = _mlContext.Data.LoadFromEnumerable(data);

        var pipeline = _mlContext.Transforms.Text
            // 1. Razbij tekst na reci
            .TokenizeIntoWords("Tokens", nameof(BookTextData.Summary))
            // 2. Ukloni stop reci
            .Append(_mlContext.Transforms.Text.RemoveDefaultStopWords("Tokens", "Tokens"))
            // 3. Mapiraj reci u brojeve (kljuceve) - potrebno za LDA
            .Append(_mlContext.Transforms.Conversion.MapValueToKey("Keys", "Tokens"))
            // 4. Kreiraj "Bag of Words" (Ngrams) - LDA zahteva ovo kao ulaz
            .Append(_mlContext.Transforms.Text.ProduceNgrams("Features", "Keys"))
            // 5. LDA algoritam
            .Append(_mlContext.Transforms.Text.LatentDirichletAllocation(
                "Topics",
                "Features",
                numberOfTopics: 3));

        try
        {
            var model = pipeline.Fit(dataView);

            // --- PRIVREMENO: Vracanje uspeha dok ne implementiramo citanje iz modela ---
            return new List<string> { "Analysis successful", "Features created", "LDA trained" };
        }
        catch (Exception ex)
        {
            return new List<string> { $"Critical Error: {ex.Message}" };
        }
    }
}