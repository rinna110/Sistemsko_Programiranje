using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Projekat3.Actors;
using Projekat3.API;
using Projekat3.Rx;
using Projekat3.Messages;
using Projekat3.Server;
using Microsoft.Extensions.Configuration;
using Akka.Configuration;
namespace Projekat3
{
    internal class FootballApp
    {
        public static async Task Run()
        {   
            //pravimo konfiguraciju za appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            string? apiKey = configuration["ApiFootball:ApiKey"];
            int leagueId = configuration.GetValue<int>("ApiFootball:LeagueId");
            int season = configuration.GetValue<int>("ApiFootball:Season");
            Logger.Log($"Liga: {leagueId}");
            Logger.Log($"Sezona: {season}");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new Exception("API kljuc nije pronadjen u appsettings.json");
            }

           var akkaConfig = ConfigurationFactory.ParseString(@"
              akka {

                actor {

                    deployment {

                        /league-actor {
                            dispatcher = league-dispatcher
                        }
                    }
                }

                league-dispatcher {

                    type = Dispatcher

                    executor = fork-join-executor

                    throughput = 100
                }
            }

             ");

            using var system = ActorSystem.Create("football-system");

            //pravimo novog aktora
            //vraca referencu IActorRef (adresa aktora)

            var leagueActor = system.ActorOf(
                 Props.Create(() => new LeagueActor()),
                 "league-actor");
            var api = new ApiFootballService(apiKey);

            var rx = new FootballRxService(api);

            //svaki put kada Rx dobije tabelu poziva standings=>
            var subscription = rx.GetStandingsStream(leagueId,season).Subscribe(standings =>
            {
                leagueActor.Tell(new UpdateStandings(standings));
            });

            var webServer = new WebServer(leagueActor);
            _=webServer.Start();

            Logger.Log("Aplikacija je pokrenuta\n");
            Console.ReadLine();

            subscription.Dispose();

            webServer.Stop();

            await system.Terminate();
        }
    }
}
