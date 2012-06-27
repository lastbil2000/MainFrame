using System;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using MainFrame.Core.Logger;

namespace MainFrame.Core.Network
{
	public class Server : IDataReceived<byte[]>
	{
		public static readonly int DEFAULT_PORT = 1234;
		
		private int _port;
		private bool _isRunning;
		private TcpListener _tcpListener;
		private IList<IClientConnection> _connectionObservers;
		
		private Object _lock = new Object();
		
		private IDictionary<string, IByteArrayConnection> _connections;
		
		public Server ()
		{
			_port = DEFAULT_PORT;
			_connections = new Dictionary<string, IByteArrayConnection> ();
			_connectionObservers = new List<IClientConnection>();
			
		}
		
		public bool IsBusy (string ip) {
			if (!_connections.ContainsKey(ip))
				throw new ApplicationException ("No ip: " + ip);
			
			return _connections[ip].Busy;
		}
		
		public void Start() 
		{
			_isRunning = true;
			ListenAsync();
		}
		
		public void AddObserver (IClientConnection observer)
		{
			_connectionObservers.Add(observer);
		}
		
		public void Stop () 
		{
			foreach (IByteArrayConnection connection in _connections.Values)
				if (connection.Busy)
					throw new ApplicationException ("Unable to Stop: connection busy: " + connection.IP);
			
			lock (_lock) {
				_isRunning = false;
				_tcpListener.Stop();
				
			}
			
			foreach (IByteArrayConnection connection in _connections.Values)
				connection.Stop();
		}
		
		private void ListenAsync() 
		{
			_tcpListener =  new TcpListener(IPAddress.Any, _port);
			_tcpListener.Start();
			
			_tcpListener.BeginAcceptSocket (EndAcceptTcpClient, _tcpListener);
		}
		
		internal void EndAcceptTcpClient(IAsyncResult asyncResult) 
		{
			TcpListener listener = asyncResult.AsyncState as TcpListener;
			TcpClient client = listener.EndAcceptTcpClient(asyncResult);
			
			lock (_lock) 
			{
				
				if (client != null &&
					client.Connected)
				{
					IByteArrayConnection connection = new TCPConnection (client, this);
					_connections.Add(connection.IP, connection);
					
					ThreadPool.QueueUserWorkItem( delegate (object obj) { connection.Start(); });
					
					foreach (IClientConnection observer in _connectionObservers)
						if (observer != null)
							observer.ClientConnected(connection.IP);
				}
			}
			
			//TODO: this value should be set relative to TIME_WAIT
			//Thread.Sleep(1000);
			//ListenAsync();
		}
		
		public void DataReceived(byte [] data, string ip)	
		{
			Log.d ("Data received with length: " + data.Length + " from: " + ip);
		}
	
		public void Write(byte [] buffer, string ip) 
		{
			
			
			if (!_connections.ContainsKey(ip))
				throw new ApplicationException ("No connection found with ip: " + ip);
			
			IByteArrayConnection connection = _connections[ip];
			connection.Write(buffer);
		}

	}
}

