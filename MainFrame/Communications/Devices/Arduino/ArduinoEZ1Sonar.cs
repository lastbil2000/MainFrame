using System;

namespace MainFrame.Devices
{
	public class ArduinoEZ1Sonar : IDistance
	{
		private static readonly int MAX_DISTANCE = 600;
		private static readonly int MIN_DISTANCE = 6;
		private static readonly int PACKAGE_EZ1 = 0x6;
		
		private static readonly int PW_PORT = 0x5;
		
		private int _pwPort;
		private ISerialCommunicationProcess _com;
		private int _lastDistance;
		
		public ArduinoEZ1Sonar (ISerialCommunicationProcess com)
		{
			if (com == null)
				throw new ArgumentNullException("ISerialCommunicationProcess must not be null.");
			
			_lastDistance = 0;
			_com = com;
			_pwPort = PW_PORT;
		}

		#region IDistance implementation
		public int Distance {
			get {
				
				_com.WaitFor();
				
				byte [] distance = _com.TransferData(new ArduinoPackage() {
					Header = new byte[2] {(byte) PACKAGE_EZ1, (byte) _pwPort},
					Data =   new byte[4] //dummy data, due to the size requirements of the package
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

