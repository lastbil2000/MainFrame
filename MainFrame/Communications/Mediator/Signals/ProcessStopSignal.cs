using MainFrame.Communication;
using MainFrame.Processes;
using System;

namespace MainFrame
{


	public class ProcessStopSignal : INervousSignal <ProcessContainer>
	{
		public string Identifier {get; set;}
		public ProcessContainer Data {get; set;}
		
		public ProcessStopSignal() 
		{
			Type = typeof(ProcessStopSignal);
		}
		public Type Type{get; set;}

	}
}

