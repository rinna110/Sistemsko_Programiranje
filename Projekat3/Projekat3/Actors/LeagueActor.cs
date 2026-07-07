using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Akka.Actor;
using Projekat3.Messages;
using Projekat3.Models;

namespace Projekat3.Actors
{
    
    internal class LeagueActor:UntypedActor
    {   
        //trenutno stanje lige
        private List<TeamStanding> _standings = new();

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case UpdateStandings update:
                    foreach (var team in update.Standings)
                    {
                        int maxPoints = team.Played * 3;
                        if(maxPoints == 0)
                        {
                            team.SuccessPercentage = 0;
                        }
                        else
                        {
                            team.SuccessPercentage = Math.Round((double)team.Points / maxPoints * 100, 2);
                        }
                    }
                    _standings=update.Standings;
                    Logger.Log($"[LeagueActor] Primljena nova tabela ({_standings.Count} timova).");
                    break;
                case GetStandingsRequest request:
                    //odgovori onom koji je poslao poruku (web server)
                    Logger.Log("[LeagueActor] Poslat odgovor sa trenutnom tabelom.");
                    Sender.Tell(new StandingsResponse(new List<TeamStanding>(_standings)));
                    break;
                default:
                    Unhandled(message);
                    break;
            }
        }

        protected override void PreStart()
        {
            Logger.Log("LeagueActor je pokrenut");
            Logger.Log($"LeagueActor koristi dispatcher: {Context.Props.Dispatcher}");
        }

        protected override void PostStop()
        {
            Logger.Log("LeagueActor je zaustavljen");
        }

    }
}
