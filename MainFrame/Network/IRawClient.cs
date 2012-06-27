using System;

namespace MainFrame.Core.Network
{
	public interface IRawClient<T>
	{
		
		void Connect (String ip, int port);
		void Disconnect ();
		void Write(T data);
		bool Ready {get;}
		bool Busy {get; }
		
	}
}

