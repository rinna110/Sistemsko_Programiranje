using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;

public class HttpServer
{
	public HttpServer(int port, int max_connections)
	{
		socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		endpoint = new IPEndPoint(IPAddress.Any, port);
	
		socket.Bind(endpoint);

		socket.Listen(max_connections);

		Logger.Log("Started HTTP server on port " + port);
	}
	
	public void listen()
	{
		while(true)
		{
			Socket req = socket.Accept();
			ThreadPool.QueueUserWorkItem(process_request, req);
		}
	}

	private bool check_path(string path)
	{
		return File.Exists(path) && !Directory.Exists(path);
	}

	private void process_request(object? param)
	{
		// never null, only passed in from listen()
		Socket req = (Socket)param!;

		NetworkStream conn_stream = new NetworkStream(req);
		BinaryReader in_stream = new BinaryReader(conn_stream);
		BinaryWriter out_stream = new BinaryWriter(conn_stream);

		String s = "";
		while(conn_stream.DataAvailable)
		{
			byte b = (byte)in_stream.ReadSByte();
			Console.Write((char)b);
			s += (char)b;
		}

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

				if(check_path(items[1]))
				{
					out_stream.Write("HTTP/1.1 200 OK\r\n".ToCharArray());
					
					// check for cache hit	
					if(items[1].EndsWith(".txt"))
					{
						Logger.Log("Converting text file to binary");


						out_stream.Write("Content-Type: application/octet-stream\r\n".ToCharArray());
						out_stream.Write("Content-Disposition: attachment\r\n".ToCharArray());

						byte[] fbytes = File.ReadAllBytes(items[1]);
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

						byte[] fbytes = File.ReadAllBytes(items[1]);
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
