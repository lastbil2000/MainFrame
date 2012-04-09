using MainFrame.Communication;
using System;

namespace MainFrame
{


	public class RunSignal : INervousSignal<bool>
	{
		public RunSignal() 
		{
			Type = typeof(RunSignal);
		}
		public Type Type{get; set;}
		public string Identifier {get; set;}
		public bool Data {get; set;}
	}
}
