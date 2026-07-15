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
using Projekat3.Models;
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

            using var system = ActorSystem.Create("football-system");

            //pravimo novog aktora
            //vraca referencu IActorRef (adresa aktora)

            var percentageActor = system.ActorOf(
                Props.Create(() => new PercentageActor()),
                "percentage-actor");

            var leagueActor = system.ActorOf(
                 Props.Create(() => new LeagueActor()),
                 "league-actor");
            var api = new ApiFootballService(apiKey);

            var rx = new FootballRxService(api);

            //svaki put kada Rx dobije tabelu poziva standings=>
            var subscription = rx.GetStandingsStream(leagueId,season).Subscribe(standings =>
            {
                leagueActor.Tell(
            new UpdateStandings(
                standings.Select(t => new TeamStanding
                {
                    Position = t.Position,
                    TeamName = t.TeamName,
                    Played = t.Played,
                    Points = t.Points
                }).ToList()));

                percentageActor.Tell(
                    new UpdatePercentages(
                        standings.Select(t => new TeamStanding
                        {
                            Position = t.Position,
                            TeamName = t.TeamName,
                            Played = t.Played,
                            Points = t.Points
                        }).ToList()));
            });

            var webServer = new WebServer(leagueActor,percentageActor);
            _=webServer.Start();

            Logger.Log("Aplikacija je pokrenuta\n");
            Console.ReadLine();

            subscription.Dispose();

            webServer.Stop();

            await system.Terminate();
        }
    }
}
