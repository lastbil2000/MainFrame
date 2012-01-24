
using System;

namespace MainFrame.Communication
{


	public interface ITriggerable
	{
		void TriggerOnEvent(object sender, EventArgs e);
	}
}
