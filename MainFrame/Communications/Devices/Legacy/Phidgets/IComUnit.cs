using System;
/// <summary>
/// The ComUnit represtents the communication device used to control other pic devices
/// through serial communication, first via the IIOBoard and later, from the ComUnit to 
/// the other SHIT 
/// 
/// </summary>

namespace MainFrame.Devices
{

	public interface IComUnit : IDevice
	{
		IIOBoard Board {get;}
		int DataPort{get;}
		int StatusPort {get;}
		
		void TransferData(IComUnitPackage data);
		
	}
}
