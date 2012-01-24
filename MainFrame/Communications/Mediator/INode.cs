using System.Collections.Generic;
using System;

namespace MainFrame.Communication
{


	public interface INode
	{
		void SetMediator (INervousSystem mediator);
		IEnumerable<Type> Subjects {get;}
		
	}
}
