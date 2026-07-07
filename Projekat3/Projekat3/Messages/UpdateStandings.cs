using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projekat3.Models;

namespace Projekat3.Messages
{
    //aktori komuniciraju preko poruka
    public sealed class UpdateStandings
    {   
        public List<TeamStanding> Standings { get; set; }
        public UpdateStandings(List<TeamStanding> standings) {
            
            Standings = standings;
        }
    }
}
