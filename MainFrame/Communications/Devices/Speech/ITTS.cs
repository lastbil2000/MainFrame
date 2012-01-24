using MainFrame.Communication;
using System;

namespace MainFrame.Devices
{


	public interface ITTS : IRequestable, IDevice
	{
		bool IsQuiet{get; set;}
		string CurrentText {get;}
		void StartUtterance(string text);
		void StopUtterance();
		
		
	}
}
