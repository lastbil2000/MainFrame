
using System;

namespace MainFrame.Devices
{


	public interface IPin : IDevice
	{
		int Index {get;}
		bool On {get; set;}
	}
}
