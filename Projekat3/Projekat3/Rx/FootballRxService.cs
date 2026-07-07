using Projekat3.API;
using Projekat3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekat3.Rx
{
    internal class FootballRxService
    {   
        //konekcija prema API-ju
        private readonly ApiFootballService _api;

        public FootballRxService(ApiFootballService api)
        {
            _api = api;
        }

        //proizvodi tok podataka (vise listi)
        public IObservable<List<TeamStanding>> GetStandingsStream(int leagueId, int season)
        {
            return Observable
                .Interval(TimeSpan.FromSeconds(10))
                .StartWith(0)
                .SelectMany( _ =>_api.GetStandingsAsync(leagueId, season));
        }

    }
}
