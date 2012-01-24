using System;
using System.Collections.Generic;

namespace MainFrame.Communication
{


	public class DTOEventHandler : IEventHandler
	{
		protected EventHandler DTOReceivedEventHandler;
		protected List<ITriggerable> iWantNotice;
			
		public DTOEventHandler ()
		{
		}
		
		public AttachDTOEvent(ITriggerable tobetriggered) //where T : IDTO
		{
			
			//list<t> enmassafunktioner
		}
		
		
		public void DataReceived (object sender, EventArgs e)
		{
			DTOReceivedEventArgs evt = (DTOReceivedEventArgs)e;
			
			foreach (ITriggerable triggerMe in iWantNotice)
			{
				if (triggerMe.ReactOnDTO(evt.newDTO))
					triggerMe.TriggerOnEvent(sender, evt);
			}
		}
		
		
	}
}
