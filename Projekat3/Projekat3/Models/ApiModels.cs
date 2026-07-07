using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Projekat3.Models
{   
    //predstavlja strukturu jsona
    public class ApiResponse
    {
        [JsonPropertyName("response")]
        public List<LeagueResponse> Responses { get; set; } = new();
    }

    public class LeagueResponse
    {
        [JsonPropertyName("league")]
        public League League { get; set; } = new();
    }

    public class League
    {
        [JsonPropertyName("standings")]
        public List<List<Standing>> Standings { get; set; } = new();
    }

    public class Standing {

        [JsonPropertyName("rank")]
        public int Rank { get; set; }

        [JsonPropertyName("points")]
        public int Points { get; set; }

        [JsonPropertyName("team")]
        public Team Team { get; set; } = new();

        [JsonPropertyName("all")]
        public AllStats All {  get; set; } = new();
    }

    public class Team
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
    }
    public class AllStats
    {
            [JsonPropertyName("played")]
            public int Played { get; set; }
    }

    
}
