using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Projekat3.Models
{   
    //koristeci ih Akka, Rx i Web Server
    public class TeamStanding
    {
        public int Position { get; set; }
        public string TeamName { get; set; }
        public int Played { get; set; }
        public int Points { get; set; }
    }
}
