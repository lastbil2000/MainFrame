using System;

namespace MainFrame.Devices
{
	public interface IWaitFor
	{
		void WaitFor(int milliSeconds = 1000);
	}
}

