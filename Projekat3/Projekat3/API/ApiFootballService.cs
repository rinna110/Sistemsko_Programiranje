using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Projekat3.Models;//

namespace Projekat3.API
{
    internal class ApiFootballService
    {
        private readonly HttpClient _httpClient;

        public ApiFootballService(string apiKey)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://v3.football.api-sports.io/");
            _httpClient.DefaultRequestHeaders.Add("x-apisports-key", apiKey);
        }

        public async Task<List<TeamStanding>> GetStandingsAsync(int leagueId, int season)
        {
            var response = await _httpClient.GetAsync($"standings?league={leagueId}&season={season}");
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonSerializer.Deserialize<ApiResponse>(json);

            //proveravamo da program ne bi pukao
            if (apiResponse == null || apiResponse.Responses.Count == 0)
            {
                return new List<TeamStanding>();
            }
            var standings = apiResponse
                     .Responses[0]
                     .League
                     .Standings[0];

            List<TeamStanding> result = new();

            foreach (var standing in standings)
            {   
                //dpdajemo u result
                result.Add(new TeamStanding
                {
                    Position = standing.Rank,
                    TeamName = standing.Team.Name,
                    Played = standing.All.Played,
                    Points = standing.Points

                });
            }
            return result;
        }


}
}
