using MainFrame.Communication;
using System;

namespace MainFrame
{


	public class RunSignal : INervousSignal<bool>
	{
		public string Identifier {get; set;}
		public bool Data {get; set;}
	}
}
