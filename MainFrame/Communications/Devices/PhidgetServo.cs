using System;
using Phidgets;
using Phidgets.Events; 
using System.Threading;
using MainFrame.Core.Logger;

namespace MainFrame.Devices
{

	
	public sealed class PhidgetServo : IServo
	{
	
		private Phidgets.Servo servo;
		private int index; //keeps track of multiple servos on a multiple servo controller 
		private bool initialized;
		
		public double MaxRotation {get { return servo.servos[index].PositionMax; } }
		public double MinRotation {get { return servo.servos[index].PositionMin; } }
		
		public double Rotation {
			get 
			{
				if (!initialized)
					throw new ApplicationException("Servo not started!");
				
				return servo.servos[index].Position;
			}
			set
			{
				if (!initialized)
					throw new ApplicationException("Servo not started!");
				
				if (value < MinRotation || value > MaxRotation)
					throw new IndexOutOfRangeException ("Rotation is out of range");
				
				servo.servos[index].Position = value;
			}
		}
		
		public bool Ready
		{
			get
			{
				if (servo.Attached)
					return initialized;
					//return servos[position].Engaged;
				
				return false;
			}
		}	

		public PhidgetServo (int index)
		{
			servo = new Phidgets.Servo();
			
			this.index = index;
			
            //Hook the basic event handlers
            servo.Attach += new AttachEventHandler(servo_Attach);
            servo.Detach += new DetachEventHandler(servo_Detach);
            servo.Error += new ErrorEventHandler(servo_Error);
			
            //hook the phidget specific event handlers
            servo.PositionChange += new PositionChangeEventHandler (servo_PositionChange);

		}
		
		public void Start ()
		{
            //open the Servo object for device connections
            servo.open();

            //Get the program to wait for a Servo to be attached
			servo.waitForAttachment();
			
			//i hate phidgets:
			Thread.Sleep(2000);
			
			while (!servo.Attached)
				Thread.Sleep(500);
			
			initialized = true;
			
		}
		
		public void Stop ()
		{
			servo.Attach -= new AttachEventHandler(servo_Attach);
            servo.Detach -= new DetachEventHandler(servo_Detach);
            servo.Error -= new ErrorEventHandler(servo_Error);
			servo.close();
		}
		
		
		
		  //Attach event handler...Display te serial number of the attached servo device
        private void servo_Attach(object sender, AttachEventArgs e)
        {
			Log.d ("servo " + index + " is attached");
			initialized = true;
        }

        //Detach event handler...Display the serial number of the detached servo device
        private void servo_Detach(object sender, DetachEventArgs e)
        {
			Log.d ("servo " + index + " deattached");
			initialized = false;
        }

        //Error event handler....Display the error description to the console
        private void servo_Error(object sender, ErrorEventArgs e)
        {
            Log.e(e.Description);
        }

        //Position CHange event handler...display which motor changed position and 
        //its new position value to the console
        private void servo_PositionChange(object sender, PositionChangeEventArgs e)
        {
			Log.d ("servo " + index + " changed position to: " + e.Position);
        }
	}
}
