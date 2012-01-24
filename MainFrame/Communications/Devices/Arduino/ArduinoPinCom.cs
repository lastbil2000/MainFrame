
using System;

namespace MainFrame.Devices
{


	public class ArduinoPinCom : IPinCom
	{
	
		public static readonly byte PACKAGE_SETPIN = 0x3;
		
		ISerialCommunicationProcess _com;
		bool [] _pins;
		bool _isTransfering;
		
		#region IPinCom implementation
		public void SetPin (int index, bool on)
		{
			_isTransfering =  true;
			
			if (! _com.Ready)
				_com.WaitFor();
			
			if (_pins[index] != on) 
			{
				_pins[index] = on;
				_com.TransferData(new ArduinoPackage() {
					Header = new byte[3] {PACKAGE_SETPIN, (byte) index, (byte) (on ? 1 : 0)},
					Data =   new byte[3]
				});
			}
			
			_isTransfering = false;
		}
		
		public bool GetPin(int index) 
		{
			return _pins[index];
		}
		
		#endregion

		public ArduinoPinCom (ISerialCommunicationProcess com)
		{
			_pins = new bool[16];
			_com = com;
		}
		
		#region IDevice implementation
		public void Start ()
		{
			
		}
		
		
		public void Stop ()
		{
			
		}
		
		
		public bool Ready {
			get {
				return _com.Ready && !_isTransfering;
			}
		}
		
		#endregion
	}
}
