using MainFrame.Communication;
using System;

namespace MainFrame.Devices
{


	public interface IHead : IDevice, IRequestable
	{
		double Rotation {get; set;}
		double Up {get; set;}
	}
}
