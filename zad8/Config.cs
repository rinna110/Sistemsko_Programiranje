public class ServerConfig
{
	public int port = 5050;
	public int maxConnections = 100;
	public int maxCacheSize = 3;
	public int threadCount = 5;
	public string rootFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "root");
}
