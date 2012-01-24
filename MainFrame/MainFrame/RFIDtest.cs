using Phidgets;
using Phidgets.Events;
using System.Collections.Generic;
using MainFrame.Devices;
using MainFrame.Communication; 
using System;
using MainFrame.Devices.Speech;

namespace MainFrame
{


	public class RFIDtest
	{
		  RFID rfid;
		INervousSystem mediator;
		
		public RFIDtest (INervousSystem nervs)
		{
			mediator = nervs;
			rfid = new RFID();
            rfid.Attach += new AttachEventHandler(rfid_Attach);
            rfid.Detach += new DetachEventHandler(rfid_Detach);
            rfid.Tag += new TagEventHandler(rfid_Tag);
            rfid.TagLost += new TagEventHandler(rfid_TagLost);
            rfid.open();
			
			
		}
		
		public void Stop () 
		{
			rfid.close();
		}
		
		void rfid_TagLost(object sender, TagEventArgs e)
        {
            Console.WriteLine("tag lost: " + e.Tag);
        }

        void rfid_Tag(object sender, TagEventArgs e)
        {
			Dictionary<string,string> test = new Dictionary<string, string>() 
			{
				{"01069334eb","Tord Wessman"},
				{"1c00c2e5f9","Rickard Askelöf"}
			};
			
			if (test.ContainsKey(e.Tag))
				mediator.Request(new EspeakTTS.SpeechSignal() { Data = "Welcome " + test[e.Tag] });
			else
				mediator.Request(new EspeakTTS.SpeechSignal() { Data = "Unknownp person. Lol. lol " });
            Console.WriteLine("tag found: " + e.Tag);
        }

        void rfid_Detach(object sender, DetachEventArgs e)
        {
			Console.WriteLine("rfid deattached");
        }

        void rfid_Attach(object sender, AttachEventArgs e)
        {
			rfid.LED = true;
			rfid.Antenna = true;
			Console.WriteLine("rfid attached");
        }

	}
}
