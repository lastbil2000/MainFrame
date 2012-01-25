using System.Threading;
using System;

namespace MainFrame.Devices
{


	public class ArduinoMotor : IServoMotor
	{
		//arduinoDeviceId: identifier recognized by the arduino software for this type of device
		public static byte arduinoDeviceId = 2;
		//arduinoPort: what outport the arduino uses to talk to the pic
		private int arduinoPort;
		//picPort: where the servo is connected on the pic controller
		private int picPort;
		
		
		
		private bool isRunning;
		private bool isTransfering;		
		private ISerialCommunicationProcess comProvider;
		private double currentVelocity;
		private int currentServoValue;		
		private int calibratedMinVal = 7;
		private int calibratedMaxVal = 195;
#pragma warning disable 0414		
		private double minRotation = 0.0;
		private double maxRotation = 180.0;		
		
		private double calibration;
		
		private static byte picHeaderSize = 4;	//the length in bits of the header transfered to the pic
		private static byte picDataSize = 8;	//the length in bits of the data transfered to the pic
		
		public ArduinoMotor (ISerialCommunicationProcess comProvider, int arduinoPort, int picPort)
		{
			calibration = -10;
			
			if (comProvider == null)
				throw new NullReferenceException("Comprovider must not be null!");
			
			this.comProvider = comProvider;
			this.arduinoPort = arduinoPort;
			this.picPort = 1 << picPort;			
		}
		
		#region IServoMotor implementation
		
		public double Calibration 
		{
			get 
			{
				return calibration;
			}
			set
			{
				calibration = value;
			}
		}
		public double Velocity {
						get {
				return currentVelocity - 90 - calibration;
			}
			set {
				
				double val = value + 90 + calibration;
				if (isTransfering)
					throw new DeviceException("Servo on picPort: " + picPort + " and arduinoPort: " + arduinoPort + " is transfering...");

				int diff = (int) Math.Abs(currentVelocity - val);
				
				int newServoValue = (int)( calibratedMinVal + (val / ( maxRotation / (calibratedMaxVal - calibratedMinVal) )));
				
				if (currentServoValue == newServoValue)
					return;
				
				isTransfering = true;

				currentServoValue = newServoValue;

				comProvider.TransferData(new ArduinoPackage() {
					Header = new byte[2] {(byte) arduinoDeviceId, (byte) arduinoPort},
					Data =   new byte[4] {picHeaderSize, picDataSize, (byte)picPort, (byte)(currentServoValue)} // 
				});
				
				Console.WriteLine("current value: " + (byte)currentServoValue);

				//Wait until servo has rotated
				Thread.Sleep(diff * 2 < 20 ? 20 : diff*2); //TODO: calibrate 
				
				currentVelocity = val;
				isTransfering = false;
			}

		}
		
		#endregion
		#region IDevice implementation
		public void Start ()
		{
			if (!comProvider.Ready) 
			{
				throw new DeviceException("ComProvider " + comProvider.ToString() + " is not ready");
			}
			isRunning = true;
			Velocity = 0;
		}
		
		
		public void Stop ()
		{
			Velocity = 0;
			isRunning = false;
		}
		
		
		public bool Ready {
			get {
				return !isTransfering && isRunning;
			}
		}
		
		#endregion

	}
}
