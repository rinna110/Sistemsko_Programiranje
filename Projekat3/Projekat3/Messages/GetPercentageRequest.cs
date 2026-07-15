using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekat3.Messages
{
    public sealed class GetPercentageRequest
    {
        public string TeamName { get; }

        public GetPercentageRequest(string teamName)
        {
            TeamName = teamName;
        }
    }
}
