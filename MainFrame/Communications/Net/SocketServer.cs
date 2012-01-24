using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace MainFrame.Communication.Net
{
	
	public class SocketServer<T>
		where T : ISocketConnection, new ()
	{
		private int port;
		private Socket listenSocket;

		private Thread listenerThread;
		private List<ISocketConnection> connections; 
		private List<ITriggerable> receiveTriggerables;
		
		public int Port {get {return port; }}
		public bool IsRunning;
		
		public SocketServer (int port)
		{
			this.port = port;
			listenerThread = new Thread(ListenerThread);
			connections = new List<ISocketConnection>();
			
			
		}
		
		public void AddTriggerable(ITriggerable triggerable)
		{
			if (receiveTriggerables == null)
				receiveTriggerables = new List<ITriggerable>();
			
			receiveTriggerables.Add(triggerable);
		}
		
		public void Start()
		{
			//Console.WriteLine("hej");
			//return;
			IsRunning = true;
			listenSocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
			
			listenSocket.Bind(new IPEndPoint(IPAddress.Any, port));
			listenSocket.Listen(10);
			listenSocket.Blocking = true;
			
			listenerThread.Start();
		}
		
		public void Stop()
		{
			IsRunning = false;
			foreach (ISocketConnection connection in connections)
				connection.Stop();
			listenSocket.Close();
			//listenSocket.Disconnect(false);
		}

		private void ListenerThread ()
		{
			Socket client = null;
			while (IsRunning)
			{
				//listenSocket.SetSocketOption(SocketOptionLevel.Socket,SocketOptionName.
				Console.WriteLine("Listening...");
				try
				{
					client = listenSocket.Accept();
				}
				catch (SocketException ex)
				{
					//interupted by the Close() call. hej pappa
					if ((SocketError)ex.ErrorCode == SocketError.Interrupted)
						return;
					else
						throw ex;
				}
				IPEndPoint newClient = (IPEndPoint)client.RemoteEndPoint;
				Console.WriteLine("Connected with {0} at port {1}",newClient.Address, newClient.Port);

				ISocketConnection connection = new T();
				connections.Add(connection);
				/*
				connection.SetSocket(client);
				if (!connections.Contains<T>(connection))
				{
					
				}
				
				else
				{<T>
					//throw exception here directly?
					connection = (from c in connections where c.Equals(connection) select c).FirstOrDefault();
					
					if (connection.IsTransfering)
						throw new ApplicationException("Transfer in progres for client with ip: " + connection.Address.ToString());
					else
						connection.SetSocket(client);
				}
				*/
				
				connection.DataReceived += DataReceived;
				ThreadStart starter = delegate { connection.Start(client); };
				Thread t = new Thread(starter);
				t.Start();
				
			}
			
		}

		private void DataReceived (object sender, EventArgs e)
		{
			foreach (ITriggerable triggerable in receiveTriggerables)
				triggerable.TriggerOnEvent(sender, e);
		}
		
		
	}
}
