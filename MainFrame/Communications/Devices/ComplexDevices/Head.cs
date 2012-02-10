using MainFrame.Communication; 
using System.Threading;
using System;

namespace MainFrame.Devices
{


	public class Head : IHead
	{
		private IServo _horizontal;
		private IServo _vertical;
#pragma warning disable 0414		
		private INervousSystem _mediator;
		private bool _isRotating;
		
		#region IRequestable implementation
		public T Request<T> (INervousSignal<T> message)
		
		{
			if (message != null) 
			{
				_isRotating = true;
				HeadPosition position = ((HeadPositionSignal)message).Data;
				
				while (!_horizontal.Ready)
					Thread.Sleep(100);
				
				_horizontal.Rotation = position.Rotation;
				
				while (!_vertical.Ready)
					Thread.Sleep(100);
				
				_vertical.Rotation = position.Up;

				while (!_vertical.Ready)
					Thread.Sleep(100);
				
				_isRotating = false;
			}
			
			return (T)(object) new HeadPosition {Rotation = this.Rotation, Up = this.Up, Ready = this.Ready};
			
		}
		
		#endregion

		public Head (IServo horizontal, IServo vertical)
		{
			this._horizontal = horizontal;
			this._vertical = vertical;
			
		}
		
		public double Rotation {
			get {
				return _horizontal.Rotation;
			}
			set {
				_horizontal.Rotation = value;
			}
		}
		
		public double Up {
			get {
				return 180 - _vertical.Rotation;
			}
			set {
				_vertical.Rotation = 180 - value;
			}
		}
		
		#region INode implementation
		public void SetMediator (INervousSystem mediator)
		{
			this._mediator = mediator;
		}
		
		public struct HeadPosition 
		{
			public double Rotation;
			public double Up;
			public bool Ready;
		}
		
		public class HeadPositionSignal : INervousSignal<Head.HeadPosition> 
		{
			public string Identifier {get; set;}
			public Head.HeadPosition Data {get; set;}
		}
		
		public System.Collections.Generic.IEnumerable<Type> Subjects {
			get {
				yield return typeof(HeadPositionSignal);
			}
		}
		
		#endregion
		#region IDevice implementation
		public void Start ()
		{
			
		}
		
		
		public void Stop ()
		{
			
		}
		
		
		public bool Ready {
			get {
				return _horizontal.Ready && _vertical.Ready && !_isRotating;
			}
		}
		
		#endregion
	}
}
