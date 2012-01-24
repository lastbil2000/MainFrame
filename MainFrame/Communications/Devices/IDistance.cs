using System;

namespace MainFrame.Devices
{
	public interface IDistance : IDevice
	{
		int Distance {get;}
		int MaxDistance {get;}
		int MinDistance {get; }
	}
}
