using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;
using GutendexApp.Models;
using System.Linq;
using GutendexApp.Services;

namespace GutendexApp.Actors;

public class CoordinatorActor : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    //za svakog autora jedan aktor
    //ime autora,referenca na aktora
    private readonly Dictionary<string, IActorRef> _authorActors = new();
   
  
    public CoordinatorActor()
    {
        // var rxService = new RxService(new HttpClient());
        Receive<FetchBooksByAuthor>(request =>
        {
            var authorKey = request.AuthorName.ToLowerInvariant();
          //prvo provera da li vec imamo aktora za tog autora
            if (!_authorActors.TryGetValue(authorKey, out var actor))
            {
                _log.Info($"[Coordinator] Autor '{request.AuthorName}' nije pronađen. Kreiram novog aktora.");

                // Kreiranje child aktora
                actor = Context.ActorOf(Props.Create(() => new AuthorActor(request.AuthorName)), $"author-{authorKey}");
                _authorActors[authorKey] = actor;
            }

            // Forward web server kao onog koji trazi podatke 
            // tako da AuthorActor moze direktno odgovoriti Serveru.
            actor.Forward(request);
        });

//ako se aktor duze vreme ne koristi
         Receive<Passivate>(msg =>
        {
          //trazimo tog aktora na osnovu reference
          var itemToRemove = _authorActors.FirstOrDefault(x => x.Value.Equals(msg.ActorRef));
    
         if (itemToRemove.Key != null)
          {
            _authorActors.Remove(itemToRemove.Key);
             _log.Info($"Aktor za {itemToRemove.Key} uklonjen iz registra.");
           }

           // gasimo aktora
           Context.Stop(msg.ActorRef);
          });
    }
}