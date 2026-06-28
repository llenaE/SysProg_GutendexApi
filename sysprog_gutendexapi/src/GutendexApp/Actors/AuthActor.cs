using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using Akka.Actor;
using Akka.Event;
using GutendexApp.Actors;
using GutendexApp.Models;

namespace GutendexApp.Actors;


public class AuthorActor:ReceiveActor
{
    
    private readonly ILoggingAdapter _log = Context.GetLogger();
    
}