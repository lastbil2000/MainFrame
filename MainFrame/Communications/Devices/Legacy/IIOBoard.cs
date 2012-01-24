using Phidgets;
using Phidgets.Events;
using System;

namespace MainFrame.Devices
{

	public enum IOPortTypes {
		DigitalOutput,
		AnalogueOutput,
		AnalogueInput
	}
	
	public interface IIOBoard : IDevice
	{
		InterfaceKit InterfaceKit {get;}
		void SetDigitalOutput(int outputIndex, bool on);
		void SetSensitivity (int outputIndex, int sensorValue);
		int GetSensorValue (int outputIndex);
		void OccupySlot (IIODevice device, IOPortTypes portType);
		
		int PortMin {get;}
		int PortMax {get;}
		
		event EventHandler AnalogueInputChanged;
		event EventHandler DigitalInputChanged;
		event EventHandler ErrorOccured;
	}
}
