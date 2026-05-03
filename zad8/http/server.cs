using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Collections.Concurrent;

public class HttpServer
{
	private const int MAX_CACHE_SIZE = 10;
	private readonly string rootFolder = "root";

	//zahtev klijenata
	private Queue<Socket> requestQueue=new Queue<Socket>();
	private object queueLock=new object();

	private readonly ConcurrentDictionary<string, byte[]> cache=new ConcurrentDictionary<string, byte[]>();
	private readonly ConcurrentDictionary<string,object> filelocks=new ConcurrentDictionary<string, object>();


	public HttpServer(int port, int max_connections)
	{
		socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		endpoint = new IPEndPoint(IPAddress.Any, port);
	
		socket.Bind(endpoint);

		socket.Listen(max_connections);

		Logger.Log("Started HTTP server on port " + port);

		int br_zahteva=5;

		for(int i=0; i<br_zahteva; i++)
		{
			Thread t = new Thread(Worker);
			t.Start();
		}
	}
	
	public void listen()
	{
		while(true)
		{
			Socket req = socket.Accept();
			//ThreadPool.QueueUserWorkItem(process_request, req);
			//kriticna sekcija
			lock (queueLock)
			{
				requestQueue.Enqueue(req);
				Monitor.Pulse(queueLock);
			}
			
		}
	}

	/*private bool check_path(string path)
	{
		return File.Exists(path) && !Directory.Exists(path);
	}*/

	private void EnsureCacheLimit()
	{
		if (cache.Count >= MAX_CACHE_SIZE)
		{
			var firstKey=cache.Keys.First();
			cache.TryRemove(firstKey, out _);
			Logger.Log("CACHE EVICTED: "+ firstKey);
		}
	}

	private void Worker()
	{	
		
		while (true)
		{
			Socket req;
			
			//kriticna sekcija za pristup queue
			lock (queueLock)
			{
                //ceka na nove zahteve
                while (requestQueue.Count == 0)
				{
					Monitor.Wait(queueLock);
				}
				req=requestQueue.Dequeue();

			}
			process_request(req);
		}
	}

	private void process_request(object? param)
	{
		// never null, only passed in from listen()
		Socket req = (Socket)param!;

		NetworkStream conn_stream = new NetworkStream(req);
		BinaryReader in_stream = new BinaryReader(conn_stream);
		BinaryWriter out_stream = new BinaryWriter(conn_stream);

		String s = "";
		byte[] buffer = new byte[1024];
		int bytesRead;
		while((bytesRead=in_stream.BaseStream.Read(buffer,0,buffer.Length))>0)
		{
			//byte b = (byte)in_stream.ReadSByte();
			//Console.Write((char)b);
			//s += (char)b;
			s += Encoding.UTF8.GetString(buffer, 0, bytesRead);
			if (s.Contains("\r\n\r\n"))//kraj HTTP headera
				break;
		}
		Logger.Log("RAW REQUEST");
		Logger.Log(s);

		if(!s.StartsWith("GET"))
		{
			string resp_str = "HTTP/1.1 405 Method Not Allowed\r\n";
			out_stream.Write(resp_str.ToCharArray());
		}
		else
		{
			string[] items = s.Split();

			if(items.Length >= 2 && items[0] == "GET")
			{
				Logger.Log("Requested path: " + items[1]);

				string path = items[1].TrimStart('/');
				string fullpath=Path.Combine(rootFolder, path);

				if(File.Exists(fullpath))
				{
					out_stream.Write("HTTP/1.1 200 OK\r\n".ToCharArray());
					byte[] fbytes;
                    
                    //kes deo
                    //u slucaju zahteva za istim resursom, obrada se izvsava samo jednom
                    if (cache.TryGetValue(fullpath, out fbytes))
					{
						Logger.Log("CACHE HIT: " + fullpath);
					}
					else
					{
						Logger.Log("CACHE MISS: " + fullpath);
						object lockObj = filelocks.GetOrAdd(fullpath,new object());
						
						lock (lockObj)
						{
							if(!cache.TryGetValue(fullpath, out fbytes))
							{
								//IO kriticna operacija
								fbytes=File.ReadAllBytes(fullpath);
								EnsureCacheLimit();//kontrola velicine kesa
								cache[fullpath] = fbytes;
							}
						}
					}
					// check for cache hit	
					if(fullpath.EndsWith(".txt"))
					{
						Logger.Log("Converting text file to binary");


						out_stream.Write("Content-Type: application/octet-stream\r\n".ToCharArray());
						out_stream.Write("Content-Disposition: attachment\r\n".ToCharArray());

						//byte[] fbytes = File.ReadAllBytes(items[1]);
						// store into cache

						out_stream.Write(("Content-Length: " + fbytes.Length + "\r\n").ToCharArray());
						out_stream.Write("\r\n".ToCharArray());
						out_stream.Write(fbytes);
					}
					else
					{
						Logger.Log("Converting binary file to text");

						out_stream.Write("Content-Type: text/plain\r\n".ToCharArray());
						out_stream.Write("Content-Disposition: attachment\r\n".ToCharArray());

						//byte[] fbytes = File.ReadAllBytes(items[1]);
						string text = Encoding.UTF8.GetString(fbytes);
						// store into cache
						
						var clen = Encoding.UTF8.GetByteCount(text);
						out_stream.Write(("Content-Length: " + clen + "\r\n").ToCharArray());
						out_stream.Write("\r\n".ToCharArray());
						out_stream.Write(text.ToCharArray());
					}
				}
				else
				{
					Logger.Log("File " + items[1] + " not found");
					string resp_str = "HTTP/1.1 404 Not Found\r\n";
					out_stream.Write(resp_str.ToCharArray());

					// send error message as response body?
				}
			}	
		}

		out_stream.Flush();
		out_stream.Close();
		in_stream.Close();
		conn_stream.Close();
		req.Close();
	}

	private Socket socket;
	private IPEndPoint endpoint;
};
