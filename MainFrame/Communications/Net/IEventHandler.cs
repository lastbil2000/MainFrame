
using System;

namespace MainFrame.Communication
{


	public interface IEventHandler
	{
		void DataReceived (object sender, EventArgs e);
	}
}
