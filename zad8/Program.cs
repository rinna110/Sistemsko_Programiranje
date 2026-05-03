const int server_port = 5050;
const int max_connections = 100;

HttpServer server = new HttpServer(server_port, max_connections);
Logger.Log("Starting HTTP server...");
Console.WriteLine("Server running on port " + server_port);
Console.WriteLine("Max connections:"+ max_connections);
server.listen();
