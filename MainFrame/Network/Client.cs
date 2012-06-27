using System;
using System.Net.Sockets;
using System.Net;
using MainFrame.Core.Logger;
using System.Threading;

namespace MainFrame.Core.Network
{
	public class Client : IClient, IDataReceived<byte []>
	{
		private IByteArrayConnection _connection;
		private Thread _connectionThread;
		
		public Client ()
		{
		}
		
		public bool Ready 
		{
			get 
			{
				return _connection != null &&
					   _connection.Ready;
			}
		}
		
		public bool Busy
		{
			get 
			{
				return _connection != null &&
					   _connection.Busy;
			}
		}
		
		
		public void Disconnect() 
		{
			if (!Ready)
				throw new ApplicationException ("Unable to Disconnect: client not Ready");
			if (_connection.Busy)
				throw new ApplicationException ("Unable to Disconnect: connection busy");
			
			_connection.Stop();
		}
		
		public void Connect (String ip, int port) 
		{
			TcpClient client = new TcpClient();
			IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

			client.Connect(serverEndPoint);
			_connection = new TCPConnection(client, this);
			_connectionThread = new Thread(_connection.Start);
			_connectionThread.Start();
			
		}
		
		public void DataReceived (byte[] data, string ip)
		{
			Log.d ("Fick: " + data.Length);
		}
		
		
		public void Write (byte [] data) 
		{
			_connection.Write(data);
		}
	}
}

