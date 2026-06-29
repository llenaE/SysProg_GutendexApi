using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;
using GutendexApp.Models;

namespace GutendexApp.Actors;

public class CoordinatorActor : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    //za svakog autora jedan aktor
    //ime autora,referenca na aktora
    private readonly Dictionary<string, IActorRef> _authorActors = new();
    public CoordinatorActor()
    {
        Receive<FetchBooksByAuthor>(request =>
        {
            var authorKey = request.AuthorName.ToLower();

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
    }
}