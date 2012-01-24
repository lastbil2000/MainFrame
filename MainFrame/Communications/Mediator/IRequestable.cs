
using System;

namespace MainFrame.Communication
{


	public interface IRequestable : INode
	{
		T Request<T>(INervousSignal<T> message);// where T : new();
	}
}
