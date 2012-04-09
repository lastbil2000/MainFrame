using MainFrame.Communication;
using System;

namespace MainFrame
{


	public class GetSignal<T> : INervousSignal<T>
	{
		public string Identifier{get; set;}
		public T Data {get; set;}
		
		public GetSignal() 
		{
			Type = typeof(GetSignal<T>);
		}
		public Type Type{get; set;}
	}
}
