using System.Threading;
using System;

namespace MainFrame.Devices
{


	public class ArduinoServo : IServo
	{
		//arduinoDeviceId: package identifier recognized by the arduino software for this type of transfer
		public static byte setOutputPinPackage = 0x2;
		//arduinoPort: what outport the arduino uses to talk to the pic
		private int _arduinoPort;
		//picPort: where the servo is connected on the pic controller
		private int _picPort;

		private double _minRotation = 0.0;
		private double _maxRotation = 180.0;
		private double _currentRotation;
		private int _currentServoValue;		
		private int _calibratedMinVal = 7;
		private int _calibratedMaxVal = 195;
		private bool _isTransfering;
		
		private static byte picHeaderSize = 4;	//the length in bits of the header transfered to the pic
		private static byte picDataSize = 8;	//the length in bits of the data transfered to the pic
		
		ISerialCommunicationProcess _com;
		
		public ArduinoServo (ISerialCommunicationProcess comProvider, int arduinoPort, int picPort)
		{
			_com = comProvider;
			this._arduinoPort = arduinoPort;
			this._picPort = 1 << picPort;
		}
		
		public double Rotation {
			get {
				return _currentRotation;
			}
			set {
				
				_com.WaitFor();
				
				if (_isTransfering)
					throw new DeviceException("Servo on picPort: " + _picPort + " and arduinoPort: " + _arduinoPort + " is transfering...");

				//int diff = (int) Math.Abs(_currentRotation - value);
				
				int newServoValue = (int)( _calibratedMinVal + (value / ( _maxRotation / (_calibratedMaxVal - _calibratedMinVal) )));
				
				if (_currentServoValue == newServoValue)
					return;
				
				_isTransfering = true;

				_currentServoValue = newServoValue;
				Console.WriteLine("Servo at: " + value);
				
				//TODO: better solution here.. i'm tired (var utkommenterat 2 rader nedan!)
				
				//if (!comProvider.IsReady);
				//	throw new DeviceException ("Could not set servo value: ISerialCommunicationProvider was busy.");
				
				_com.TransferData(new ArduinoPackage() {
					Header = new byte[2] {(byte) setOutputPinPackage, (byte) _arduinoPort},
					Data =   new byte[4] {picHeaderSize, picDataSize, (byte)_picPort, (byte)(_currentServoValue)} // 
				});
				
				//Console.WriteLine("current value: " + (byte)_currentServoValue);

				//Wait until servo has rotated
				//Thread.Sleep(diff * 2 < 20 ? 20 : diff*2); //TODO: calibrate 
				
				_currentRotation = value;
				_isTransfering = false;
			}
		}
		
//		private byte InvertBits (byte data, int count)
//		{
//			byte ret = 0;
//			
//			for (int serialBit = 0; serialBit < count; serialBit++)
//			{
//				ret += (byte)(  (data & 1) << 
//					Convert.ToByte((count - 1) - serialBit));
//				data = (byte)(data >> 1);
//			}
//			
//			return ret;
//			
//		}
		
		public double MinRotation {
			get {
				return _minRotation;
			}
		}
		
		
		public double MaxRotation {
			get {
				return _maxRotation;
			}
		}
		

		public void Start ()
		{
			//TODO: send configuration
			return;
		}
		
		
		public void Stop ()
		{
			return;
		}
		
		
		public bool Ready {
			get {
				return _com.Ready && !_isTransfering;
			}
		}
		

		#region IWaitFor implementation
		public void WaitFor (int milliSeconds)
		{
			_com.WaitFor();
		}
		#endregion
	}
}
