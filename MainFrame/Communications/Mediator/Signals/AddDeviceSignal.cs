using MainFrame.Communication;
using MainFrame.Devices;
using System;

namespace MainFrame
{


	public class AddDeviceSignal :  INervousSignal <DeviceContainer>
	{
		public string Identifier {get; set;}
		public DeviceContainer Data {get; set;}
	}
}
