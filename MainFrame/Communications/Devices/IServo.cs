
using System;

namespace MainFrame.Devices
{


	public interface IServo : IDevice
	{
		double Rotation {get; set;}
		double MinRotation {get;}
		double MaxRotation {get;}
	}
}
