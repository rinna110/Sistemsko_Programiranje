ServerConfig config = new ServerConfig();

Logger.Log("Starting HTTP server...");
HttpServer server = new HttpServer(config);
server.listen();
