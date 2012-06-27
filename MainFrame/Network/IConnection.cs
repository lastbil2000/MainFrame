using System;

namespace MainFrame.Core.Network
{
	internal interface IConnection<T>
	{
		
		void Stop();
		void Start();
		
		string IP {get;}
		bool Ready {get; }
		bool Busy {get; }
		void Write (T buffer);
		
	}
}

