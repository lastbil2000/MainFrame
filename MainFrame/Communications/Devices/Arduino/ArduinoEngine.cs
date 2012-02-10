using System;

namespace MainFrame.Devices
{
	public class ArduinoEngine : IServoMotor
	{
		private IServo _servo;
		private IPin _powerPin;
		private double _calibration;
		
		public ArduinoEngine (IServo servo, IPin powerPin)
		{
			_servo = servo;
			_powerPin = powerPin;
		}

		#region IServoMotor implementation
		public double Velocity {
			get {
				return _servo.Rotation - _servo.MaxRotation/2 - _calibration;
			}
			set {
				if (value == 0) {
					Stop();
				}
				else {
					
					double val = value + 90 + _calibration;
					
					if (val < _servo.MinRotation)
						val = _servo.MinRotation;
					if (val > _servo.MaxRotation)
						val = _servo.MaxRotation;
					
					_servo.Rotation = val;
					
					_powerPin.On = true;
				}
			}
		}
		
		
		public double Calibration 
		{
			get 
			{
				return _calibration;
			}
			set
			{
				_calibration = value;
			}
		}
		#endregion

		#region IDevice implementation
		public void Start ()
		{
			_servo.Rotation = 90;
		}

		public void Stop ()
		{
			//_servo.Rotation = 90;
			_powerPin.On = false;
		}

		public bool Ready {
			get {
				return _servo.Ready && _powerPin.Ready;
			}
		}
		#endregion
	}
}

