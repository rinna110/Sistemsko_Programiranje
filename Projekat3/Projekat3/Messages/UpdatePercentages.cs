using Projekat3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekat3.Messages
{
    public sealed class UpdatePercentages
    {
        public List<TeamStanding> Standings { get; }

        public UpdatePercentages(List<TeamStanding> standings)
        {
            Standings = standings;
        }
    }
}
