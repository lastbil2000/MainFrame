using System;
using System.Threading;
using MainFrame.Communication;

namespace MainFrame.Devices
{


	public sealed class Arm3Engines : IDevice, IRequestable
	{

		private IServo _armServo;
		private IServo _rotateServo;
		private IServo _handServo;
#pragma warning disable 0414		
		private INervousSystem _mediator;
		private bool _isOpen;
		
		public int OpenRotation = 110;
				
		public Arm3Engines (IServo armServo, IServo rotateServo, IServo handServo)
		{
			if (armServo == null)
				throw new ArgumentNullException("armServo provided was null");

			if (rotateServo == null)
				throw new ArgumentNullException("rotateServo provided was null");
			
			if (handServo == null)
				throw new ArgumentNullException("handServo provided was null");

			this._armServo = armServo;
			this._rotateServo = rotateServo;
			this._handServo = handServo;
			
			/*
			if (!armServo.Ready)
				throw new DeviceException ("Arm servo not ready");
			if (!rotateServo.Ready)
				throw new DeviceException("Rotate servo not ready");
			if (!handServo.Ready)
				throw new DeviceException("Hand servo not ready");
			*/
		}
		
		public void Start ()
		{
			//solves a bug in the phidget chain
			while (!_armServo.Ready)
				Thread.Sleep(10);
			_armServo.Rotation = 90;
		}
		
		
		public void Stop ()
		{
			
		}
		
		
		public bool Ready {
			get {
				return _armServo.Ready && 
					_rotateServo.Ready &&
					_handServo.Ready;
			}
		}
		
		public void MoveHand(double degrees) 
		{
			if (degrees < 0 || degrees > OpenRotation)
				throw new DeviceException( degrees.ToString() + " is out of range.");
			
			if (degrees == OpenRotation)
				_isOpen = true;
			if (degrees == 10)
				_isOpen = false;
			
			_handServo.Rotation = degrees;
		}
		
		public bool Open 
		{
			get 
			{
				return _isOpen;
			}
			set 
			{
				if (value)
					MoveHand(OpenRotation);
				else
					MoveHand (10);
					
			}
			
		}
		
		public bool IsOpen 
		{
			get
			{
				return _isOpen;
			}
		}
		
		public double Up
		{
			get 
			{
				return _armServo.Rotation;
			}
			set 
			{
				_armServo.Rotation = value;
			}
			
		}
		
		public double Rotate
		{
			get 
			{
				return _rotateServo.Rotation;
			}
			set 
			{
				_rotateServo.Rotation = value;
			}
			
		}			
			
		public T Request<T> (INervousSignal<T> message)
		{
			if (message is GetSignal<Arm3Engines>) 
			{
				return (T)(object)this;
			}
			
			return default(T);

		}

		public void SetMediator (INervousSystem mediator)
		{
			this._mediator = mediator;
		}
		
		public System.Collections.Generic.IEnumerable<Type> Subjects {
			get {
				yield return typeof(GetSignal<Arm3Engines>);
			}
		}
		
	}
}