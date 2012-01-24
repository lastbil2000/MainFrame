
using System;

namespace MainFrame.Devices
{


	public interface IDevice
	{
		bool Ready {get;}	//The device is attached and ready
		void Start();
		void Stop();
	}
}
