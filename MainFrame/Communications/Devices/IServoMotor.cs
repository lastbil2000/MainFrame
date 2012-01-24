
using System;

namespace MainFrame.Devices
{


	public interface IServoMotor : IDevice
	{
		double Velocity {get; set;}
	}
}
