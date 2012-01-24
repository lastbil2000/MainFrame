using System.Collections.Generic;
using System;

namespace MainFrame
{


	public interface INotifyable : INode
	{
		void Notify<T>(INervousSignal <T>message);
	}
}
