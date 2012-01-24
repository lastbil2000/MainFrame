using System;

namespace MainFrame.Devices
{
	public class ArduinoSRF05Sonar : IDistance
	{
		private static readonly int MAX_DISTANCE = 600;
		private static readonly int MIN_DISTANCE = 6;
		private static readonly int PACKAGE_SRF05 = 0x5;
		
		private static readonly int ECHO_PORT = 0x3;
		private static readonly int DATA_PORT = 0x4;
		
		private int _echoPort;
		private int _dataPort;
		private ISerialCommunicationProcess _com;
		private int _lastDistance;
		
		public ArduinoSRF05Sonar (ISerialCommunicationProcess com)
		{
			if (com == null)
				throw new ArgumentNullException("ISerialCommunicationProcess must not be null.");
			
			_com = com;
			_echoPort = ECHO_PORT;
			_dataPort = DATA_PORT;
		}

		#region IDistance implementation
		public int Distance {
			get {
				
				_com.WaitFor();
				
				byte [] distance = _com.TransferData(new ArduinoPackage() {
					Header = new byte[3] {(byte) PACKAGE_SRF05, (byte) _echoPort, (byte)_dataPort},
					Data =   new byte[3] //dummy data, due to the size requirements of the package
				});
				
				if (distance != default(byte[]) &&
					distance.Length > 0 )
				{
					_lastDistance = (int) distance[0];
				}

				return _lastDistance;

			}
		}
	
		public int MaxDistance {
			get {
				return MAX_DISTANCE;
			}
		}
	
		public int MinDistance {
			get {
				return MIN_DISTANCE;
			}
		}
		#endregion
	
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
				return _com.Ready;
			}
		}
		#endregion
	}
}

