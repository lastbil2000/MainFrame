using System.Net.Sockets;
using System.Net;
using System;

namespace MainFrame.Communication.Net
{


	public interface ISocketConnection  : IEquatable<ISocketConnection>
	{
		bool IsTransfering {get;}
		IPAddress Address {get; }
		int Port {get; }
		event EventHandler DataReceived;
		void Respond (object response);
		//void SetSocket (Socket client);
		void Start(Socket client);
		void Stop();
		//bool Equals (ISocketConnection obj);
	}
}
