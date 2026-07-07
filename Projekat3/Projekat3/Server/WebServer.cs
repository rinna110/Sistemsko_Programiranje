using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Projekat3.Messages;

namespace Projekat3.Server
{
    internal class WebServer
    {
        private readonly HttpListener _listener=new();

        //referenca na LeagueActor
        private readonly IActorRef _leagueActor;
        private bool _isRunning;

        public WebServer(IActorRef leagueActor)
        {
            _leagueActor = leagueActor;
            _listener.Prefixes.Add("http://localhost:5000/");
            
        }

        public async Task Start()
        {
            _listener.Start();

            Console.WriteLine("WebServer je pokrenut");
            Console.WriteLine("http://localhost:5000/");

            while (true)
            {
                var context = await _listener.GetContextAsync();
                try
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] " +
                                      $"{context.Request.HttpMethod} " +
                                      $"{context.Request.Url.AbsolutePath}");

                    string path = context.Request.Url.AbsolutePath;

                    if (path == "/standings")
                    {
                        var response = await _leagueActor.Ask<StandingsResponse>(
                            new GetStandingsRequest(),
                            TimeSpan.FromSeconds(5));

                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] " +
                                          $"LeagueActor vratio {response.Standings.Count} timova.");

                        string responseText = JsonSerializer.Serialize(
                            response.Standings,
                            new JsonSerializerOptions
                            {
                                WriteIndented = true
                            });

                        byte[] buffer = Encoding.UTF8.GetBytes(responseText);

                        context.Response.ContentLength64 = buffer.Length;

                        await context.Response.OutputStream.WriteAsync(buffer);

                        context.Response.Close();

                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] HTTP odgovor uspesno poslat.");
                    }
                    else
                    {
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Nepostojeca ruta: {path}");

                        context.Response.StatusCode = 404;

                        byte[] buffer = Encoding.UTF8.GetBytes("404 - Not Found");

                        context.Response.ContentLength64 = buffer.Length;

                        await context.Response.OutputStream.WriteAsync(buffer);

                        context.Response.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ERROR: {ex.Message}");

                    context.Response.StatusCode = 500;

                    byte[] buffer = Encoding.UTF8.GetBytes("Internal Server Error");

                    context.Response.ContentLength64 = buffer.Length;

                    await context.Response.OutputStream.WriteAsync(buffer);

                    context.Response.Close();
                }

            }

        }
    }
}
