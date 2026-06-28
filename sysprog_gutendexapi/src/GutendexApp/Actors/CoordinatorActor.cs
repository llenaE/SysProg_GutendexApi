using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;
using GutendexApp.Models;

namespace GutendexApp.Actors;

public class CoordinatorActor: ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    //za svakog autora jedan aktor
    //ime autora,referenca na aktora
    private readonly Dictionary<string, IActorRef> _authorActors = new();

}