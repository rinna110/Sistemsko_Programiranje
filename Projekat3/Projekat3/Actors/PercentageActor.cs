using Akka.Actor;
using Projekat3.Messages;
using Projekat3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekat3.Actors
{
    internal class PercentageActor : UntypedActor
    {
        private List<TeamStanding> _standings = new();
        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case UpdatePercentages update:
                    Logger.Log("[PercentageActor] primljena tabela");
                    _standings = update.Standings;
                    break;
                case GetPercentageRequest request:
                    var selectedteam = _standings.FirstOrDefault(t =>
                        t.TeamName.Equals(request.TeamName, StringComparison.OrdinalIgnoreCase));
                    if (selectedteam == null)
                    {
                        Sender.Tell(null);
                        break;
                    }
                    int maxPoints = selectedteam.Played * 3;
                    double percentage;
                    if (maxPoints == 0)
                    {
                        percentage = 0;
                    }
                    else
                    {
                        percentage = Math.Round((double)selectedteam.Points / maxPoints * 100, 2);
                    }
                    Logger.Log($"[PercentageActor] Izracunat procenat za {selectedteam.TeamName}");
                    Sender.Tell(new PercentageResponse(
                            selectedteam.TeamName,
                            selectedteam.Position,
                            selectedteam.Points,
                            percentage));
                    break;
                default:
                    Unhandled(message);
                    break;

            }

        }
        protected override void PreStart()
        {
            Logger.Log("PercentageActor je pokrenut");
            Logger.Log($"PercentageActor koristi dispatcher: {Context.Props.Dispatcher}");
        }

        protected override void PostStop()
        {
            Logger.Log("PercentageActor je zaustavljen");
        }
    }
}
