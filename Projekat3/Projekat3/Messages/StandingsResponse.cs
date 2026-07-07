using Projekat3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekat3.Messages
{
    public sealed class StandingsResponse
    {   
        public List<TeamStanding> Standings { get; }
        public StandingsResponse(List<TeamStanding> standings)
        {
            Standings = standings;
        }
    }
}
