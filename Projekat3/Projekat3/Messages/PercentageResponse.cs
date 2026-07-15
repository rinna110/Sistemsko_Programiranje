using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekat3.Messages
{
    public sealed class PercentageResponse
    {
        public string TeamName { get; }
        public int Position { get; }
        public int Points { get; }
        public double SuccessPercentage { get; }

        public PercentageResponse(
            string teamName,
            int position,
            int points,
            double percentage)
        {
            TeamName = teamName;
            Position = position;
            Points = points;
            SuccessPercentage = percentage;
        }
    }
}
