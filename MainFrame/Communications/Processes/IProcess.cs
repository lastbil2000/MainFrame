using MainFrame.Communication;
using System;

namespace MainFrame.Processes
{


	public interface IProcess : MainFrame.Devices.IDevice
	{
		bool IsRunning {get;}
	}
}
