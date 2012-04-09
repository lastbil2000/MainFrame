
using System;

namespace MainFrame.Communication
{

	/*
	public interface INervousSignal
	{
		Type Subject {get;}
		object Data {get;}
	}
	*/
	
	public interface INervousSignal<T>// where T: new()
	{
		Type Type{get; set;}
		string Identifier {get; set;}
		T Data {get; set;}
	}
	
}
