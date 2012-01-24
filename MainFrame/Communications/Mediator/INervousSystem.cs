using System.Collections.Generic;
using System;

namespace MainFrame.Communication
{

	public interface INervousSystem : MainFrame.Devices.IDevice
	{
		void Register (IRequestable client);
		void Register (IRequestable client, string identifier);
		void Unregister (IRequestable client);
		bool Contains(IRequestable client);
		T Request<T>(INervousSignal<T> message);
		
		string GetIdentifier (IRequestable client);
	}
}
