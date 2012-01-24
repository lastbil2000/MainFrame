
using System;

namespace MainFrame.Devices
{

	
	public interface IIODevice : IDevice
	{
		IIOBoard IOBoard {get;}
		int Slot {get; set;}
	}
}
