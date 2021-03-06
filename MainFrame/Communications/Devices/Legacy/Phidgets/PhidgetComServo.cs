using System;
using System.Threading;

namespace MainFrame.Devices
{


	public class PhidgetComServo : IServo
	{
		private double minRotation = 0.0;
		private double maxRotation = 180.0;
		private double currentRotation;
		private IComUnit comUnit;
		private int comUnitPort;
		private int servoControllerPort;
		private int currentServoValue;
		private int calibratedMinVal = 7;
		private int calibratedMaxVal = 195;
		
		#region IServo implementation
		public double Rotation {
			get {
				return currentRotation;
			}
			set {
				
				int diff = (int) Math.Abs(currentRotation - value);
				
				int newServoValue = (int)( calibratedMinVal + (value / ( maxRotation / (calibratedMaxVal - calibratedMinVal) )));
				
				
				if (currentServoValue == newServoValue)
						return;
				
				currentServoValue = newServoValue;
				
				comUnit.TransferData( new ComUnitPackage ()
				{
					OutPort = comUnitPort,
					Header = servoControllerPort,
					Data = currentServoValue
				});
				
				//Wait until servo has rotated
				Thread.Sleep(diff * 2 < 20 ? 20 : diff*2); //TODO: calibrate 
				
				currentRotation = value;
			}
		}
		
		
		public double MinRotation {
			get {
				return minRotation;
			}
		}
		
		
		public double MaxRotation {
			get {
				return maxRotation;
			}
		}
		

		public void Start ()
		{
			//TODO: Autogenerated stub
		}
		
		
		public void Stop ()
		{
			//TODO: Autogenerated stub
		}
		
		
		public bool Ready {
			get {
				//TODO: fix this
				return this.comUnit.Ready;
			}
		}
		
		#endregion

		public PhidgetComServo (IComUnit comUnit, int comUnitPort, int servoControllerPort)
		{
			if (servoControllerPort < 0 || servoControllerPort > 2)
				throw new ApplicationException ("servoControllerPort is out of range.");
			if (comUnitPort < 0 || comUnitPort > 1)
				throw new ApplicationException ("comUnitPort is out of range.");
			
			this.comUnit = comUnit;
			this.comUnitPort = 1 << comUnitPort;
			this.servoControllerPort = 1 << servoControllerPort;
		}
	}
}
