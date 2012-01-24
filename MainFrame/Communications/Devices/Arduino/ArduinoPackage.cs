
using System;

namespace MainFrame.Devices
{


	public class ArduinoPackage : ISerialPackage
	{
		public byte[] Header {get; set;}
		public byte[] Data	 {get; set;}
		
		public ArduinoPackage ()
		{
		}
	}
}
