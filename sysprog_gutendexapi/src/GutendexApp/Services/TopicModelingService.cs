using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Text;
using Microsoft.ML.Trainers;
using GutendexApp.Models;

namespace GutendexApp.Services;

public class BookTextData
{
    public string Summary { get; set; }=" ";
}
public class TopicPrediction //za svaku knjigu dobijemo niz brojeva tj verovatnoca teme
{
    [VectorType]
    public float[]? Topics { get; set; }
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
    var stopWords = new HashSet<string>
{
    "summary",
    "automatically",
    "generated",
    "published",
    "book"
};

        //uklanjamo knjige koje nemaju opis, ako postoje uopste
       // var validBooks = books.Where(b => !string.IsNullOrWhiteSpace(b.Summaries)).ToList();
       
       //summaries je ipak List<string> tako vraca API
          var validBooks = books
         .Where(b => b.Summaries != null && b.Summaries.Count > 0)
         .ToList();

        if (validBooks.Count < 2)
            return new List<string> { "Not enough data." };

       var data = validBooks.Select(b =>
    {
         var text = string.Join(" ", b.Summaries).ToLower();

        //ovde uklanjamo reci koje se bukvlano nalaze u svakom opisu jer se ponavljaju svuda i onda se trenira model uvek nad istim podacima i uvek dobijemo te rezultate
        foreach (var w in stopWords)
        text = text.Replace(w, " ");

        return new BookTextData { Summary = text };
      });

        var dataView = _mlContext.Data.LoadFromEnumerable(data);

        var pipeline = _mlContext.Transforms.Text
            //razbijanje tekst na reci
            .TokenizeIntoWords("Tokens", nameof(BookTextData.Summary))
            //uklanja veznike i slicno
             .Append(_mlContext.Transforms.Text.RemoveDefaultStopWords("CleanTokens", "Tokens"))
             .Append(_mlContext.Transforms.Text.RemoveStopWords("CleanTokens","Tokens",stopWords.ToArray()))
            .Append(_mlContext.Transforms.Conversion.MapValueToKey("Keys", "CleanTokens"))
            // kreira vektor ucestalosti pojavljivanja reci
            .Append(_mlContext.Transforms.Text.ProduceNgrams("Features", "Keys"))
            // LDA algoritam
            .Append(_mlContext.Transforms.Text.LatentDirichletAllocation(
                "Topics",
                "Features",
                numberOfTopics: 3));//ograniceno za sad da pronalazi 3 najcesce teme

        try
        {
            var model = pipeline.Fit(dataView);//model ce da uci iz tekstova koji se proslede
            var transformed = model.Transform(dataView);//ovako ce da primeni ono sto je naucio

            var predictions = _mlContext.Data
                .CreateEnumerable<TopicPrediction>(transformed, reuseRowObject: false)
                .ToList(); //rezultate koje model vraca pretvaramo u listu

            //grupisanje knjiga op temi
            Dictionary<int, List<string>> topicTexts = new();

            for (int i = 0; i < predictions.Count; i++)//prolazimo kroz svaku knjigu
            {
                var probs = predictions[i].Topics;//ovde imamo niz verovatnoca tema za jednu knjigu
               int topic = probs!
                           .Select((value, index) => new { value, index })
                           .OrderByDescending(x => x.value)
                           .First().index;  //pretvaramo niz u parove, sortiramo po vrednosti i vratimo najveci


                if (!topicTexts.ContainsKey(topic))
                    topicTexts[topic] = new List<string>();
                
                var dataList=data.ToList();

                topicTexts[topic].Add(dataList[i].Summary);
            }

            List<string> results = new();

            foreach (var topic in topicTexts)
            {
                var words = topic.Value
                    .SelectMany(text => text.ToLower()
                        .Split(new[]
                        {
                            ' ', '.', ',', ';', ':', '!', '?',
                            '-', '\n', '\r', '(', ')', '"', '\''
                        },
                        StringSplitOptions.RemoveEmptyEntries))
                    .Where(w => w.Length > 4)
                    .GroupBy(w => w)
                    .OrderByDescending(g => g.Count())
                    .Take(6)
                    .Select(g => g.Key);

                results.Add($"Topic {topic.Key}: {string.Join(", ", words)}");
            }

            //jedan Topic je grupa reci koje se cesto javljaju zajedno

            return results;
        }

    
        
        catch (Exception ex)
        {
            return new List<string> { $"Critical Error: {ex.Message}" };
        }
    }
}