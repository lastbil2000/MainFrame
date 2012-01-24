using MainFrame.Processes;
using System;

namespace MainFrame.Devices
{


	public interface ISerialCommunicationProcess : IProcess, MainFrame.Devices.IWaitFor
	{	
		//transfer a package of data
		byte[] TransferData (ISerialPackage data);
		
		//raw transfer of bytes to unit
		byte[] TransferBytes(byte [] bytes);

		//the interval needed to complete a data transfer (in ms)
		int ReadInterval {get; }
	}
}
