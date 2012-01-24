using System;
using System.Threading;

namespace MainFrame.Devices
{
	public class ArduinoIRSensor : IDistance
	{

		//the serial commincation unit
		private ISerialCommunicationProcess _com;
		//returns the last measured distance
		private int _lastDistance;
		//the analoge pin on the arduino board to which the IR unit is connected
		private int _arduinoPort;
		//arduinoDeviceId: package identifier recognized by the arduino software for this type of transfer
		private static readonly int PACKAGE_2Y0A02 = 0x4;
		
		public ArduinoIRSensor (ISerialCommunicationProcess arduino, int arduinoAnalogInputPin)
		{
			_com = arduino;
			_arduinoPort = arduinoAnalogInputPin;
		}
		
		public int MaxDistance {get { return 150; }}
		public int MinDistance {get { return 20; }}
		
		public int Distance
		{
			get 
			{
				_com.WaitFor();
				
				byte [] distance = _com.TransferData(new ArduinoPackage() {
					Header = new byte[2] {(byte) PACKAGE_2Y0A02, (byte) _arduinoPort},
					Data =   new byte[4] //dummy data, due to size requirements of the package
				});
				
				if (distance != default(byte[]) &&
					distance.Length > 0 )
				{
					_lastDistance = (int) distance[0];
				}
	
				return _lastDistance;
			}
		}
		
		#region IDevice implementation
		public void Start ()
		{
			if (!_com.IsRunning)
				throw new DeviceException("ISerialCommunicationProcess is not running!");			
			//TODO: send configuration to arduino
		}

		public void Stop ()
		{

		}

		public bool Ready {
			get {
				return _com.IsRunning;
			}
		}
		#endregion
	}
}

