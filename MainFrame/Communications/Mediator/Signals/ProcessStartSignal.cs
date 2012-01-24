using MainFrame.Communication;
using MainFrame.Processes;
using System;

namespace MainFrame
{


	public class ProcessStartSignal : INervousSignal <ProcessContainer>
	{
		public string Identifier {get; set;}
		public ProcessContainer Data {get; set;}

	}
}
