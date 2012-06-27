using System;

namespace MainFrame.Core.Network
{
	public interface IClientConnection
	{
		
		void ClientConnected(string ip);
		void ClientDisconnected (string ip);
	}
}

