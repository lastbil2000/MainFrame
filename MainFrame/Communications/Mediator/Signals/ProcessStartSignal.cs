using MainFrame.Communication;
using MainFrame.Processes;
using System;

namespace MainFrame
{


	public class ProcessStartSignal : INervousSignal <ProcessContainer>
	{
		public string Identifier {get; set;}
		public ProcessContainer Data {get; set;}
		
		public ProcessStartSignal() 
		{
			Type = typeof(ProcessStartSignal);
		}
		public Type Type{get; set;}

	}
}
