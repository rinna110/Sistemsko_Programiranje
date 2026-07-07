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

            using var system = ActorSystem.Create("football-system");

            //pravimo novog aktora
            //vraca referencu IActorRef (adresa aktora)
            var leagueActor = system.ActorOf(
                 Props.Create(() => new LeagueActor()),
                 "league-actor");
            var api = new ApiFootballService("apiKey");

            var rx = new FootballRxService(api);

            //svaki put kada Rx dobije tabelu poziva standings=>
            var subscription = rx.GetStandingsStream(39, 2022).Subscribe(standings =>
            {
                leagueActor.Tell(new UpdateStandings(standings));
            });

            await Task.Delay(5000);

            var response = await leagueActor.Ask<StandingsResponse>(new GetStandingsRequest());

            Console.WriteLine();
            Console.WriteLine("---------TABELA-----------");

            foreach(var team in response.Standings)
            {
                Console.WriteLine(
        $"{team.Position}. {team.TeamName} - {team.Points} bodova - {team.SuccessPercentage:F2}%");
            }

            var webServer = new WebServer(leagueActor);
            _=webServer.Start();

            Console.WriteLine("Aplikacija je pokrenuta\n");
            Console.ReadLine();

            subscription.Dispose();
        }
    }
}
