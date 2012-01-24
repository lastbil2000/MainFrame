
using System;

namespace MainFrame.Devices
{


	public interface ISerialPackage
	{
		byte[] Header {get; set;}
		byte[] Data	 {get; set;}
				
	}
}
