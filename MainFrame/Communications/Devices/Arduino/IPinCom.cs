
using System;

namespace MainFrame.Devices
{


	public interface IPinCom : IDevice
	{
		void SetPin (int index, bool on);
		bool GetPin (int index);
		
	}
}
