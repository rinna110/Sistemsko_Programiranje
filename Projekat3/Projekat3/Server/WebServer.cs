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

        public WebServer(IActorRef leagueActor)
        {
            _leagueActor = leagueActor;
            _listener.Prefixes.Add("http://localhost:5000/");
            
        }

        public async Task Start()
        {
            _listener.Start();

            Logger.Log("WebServer je pokrenut");
            Logger.Log("http://localhost:5000/");

            

            while (true)
            {
                HttpListenerContext context;

                try
                {
                    context = await _listener.GetContextAsync();
                }
                catch (HttpListenerException)
                {
                    break;
                }
               
                try
                {
                    
                    Logger.Log(context.Request.HttpMethod + context.Request.Url.AbsolutePath);

                    string path = context.Request.Url.AbsolutePath;

                    if (path == "/standings")
                    {
                        var response = await _leagueActor.Ask<StandingsResponse>(
                            new GetStandingsRequest(),
                            TimeSpan.FromSeconds(5));

                       Logger.Log($"LeagueActor vratio {response.Standings.Count} timova.");

                        string responseText = JsonSerializer.Serialize(
                            response.Standings,
                            new JsonSerializerOptions
                            {
                                WriteIndented = true
                            });

                        byte[] buffer = Encoding.UTF8.GetBytes(responseText);

                        context.Response.StatusCode = 200;
                        context.Response.ContentType = "application/json";
                        context.Response.ContentEncoding = Encoding.UTF8;

                        context.Response.ContentLength64 = buffer.Length;

                        await context.Response.OutputStream.WriteAsync(buffer);

                        context.Response.Close();

                        Logger.Log("HTTP odgovor uspesno poslat.");
                    }
                    else
                    {
                        Logger.Log($"Nepostojeca ruta: {path}");

                        context.Response.StatusCode = 404;

                        byte[] buffer = Encoding.UTF8.GetBytes("404 - Not Found");

                   
                        context.Response.ContentType = "text/plain";
                        context.Response.ContentEncoding = Encoding.UTF8;

                        context.Response.ContentLength64 = buffer.Length;

                        await context.Response.OutputStream.WriteAsync(buffer);

                        context.Response.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"ERROR: {ex.Message}");

                    context.Response.StatusCode = 500;

                    byte[] buffer = Encoding.UTF8.GetBytes("Internal Server Error");

                    context.Response.ContentType = "text/plain";
                    context.Response.ContentEncoding = Encoding.UTF8;

                    context.Response.ContentLength64 = buffer.Length;

                    await context.Response.OutputStream.WriteAsync(buffer);

                    context.Response.Close();
                }

            }

        }

        public void Stop()
        {
           
            _listener.Stop();
           Logger.Log("WebServer zausavljen");
        }
    }
}
