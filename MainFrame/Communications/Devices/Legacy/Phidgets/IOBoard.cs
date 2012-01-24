using System.Text;
using System;
using Phidgets;
using Phidgets.Events;
using System.Collections.Generic;

namespace MainFrame.Devices
{


	public sealed class IOBoard : IIOBoard
	{
		private InterfaceKit ifKit;
		private bool initialized;
		
		public int PortMin {get {return 0;} }
		public int PortMax {get {return ifKit.outputs.Count -1;} }
		
		public event EventHandler AnalogueInputChanged;
		public event EventHandler DigitalInputChanged;
		public event EventHandler ErrorOccured;
		//TODO: fix device occupations
		//private Dictionary<int,bool>
		
		public bool Ready
		{
			get
			{
				return ifKit.Attached && initialized;
			}
		}	
		public InterfaceKit InterfaceKit {get {return ifKit;}}

		
		
		public IOBoard ()
		{
			
			
            ifKit = new InterfaceKit();
			
            ifKit.Attach += new AttachEventHandler(ifKit_Attach);
            ifKit.Detach += new DetachEventHandler(ifKit_Detach);
            ifKit.Error += new ErrorEventHandler(ifKit_Error);

            ifKit.InputChange += new InputChangeEventHandler(ifKit_InputChange);
            ifKit.OutputChange += new OutputChangeEventHandler(ifKit_OutputChange);
            ifKit.SensorChange += new SensorChangeEventHandler(ifKit_SensorChange);

		}
		
		public void Start ()
		{
            //Open the Phidget using the command line arguments
            ifKit.open();
			//TODO: add to event instead
			ifKit.waitForAttachment();

		}
		
		public void Stop () 
		{
			ifKit.Attach -= new AttachEventHandler(ifKit_Attach);
            ifKit.Detach -= new DetachEventHandler(ifKit_Detach);
            ifKit.Error -= new ErrorEventHandler(ifKit_Error);

            ifKit.InputChange -= new InputChangeEventHandler(ifKit_InputChange);
            ifKit.OutputChange -= new OutputChangeEventHandler(ifKit_OutputChange);
            ifKit.SensorChange -= new SensorChangeEventHandler(ifKit_SensorChange);
			ifKit.close();
		}
		/*
		public void AttachDevice (IIODevice device)
		{
			
		}
		*/
		
		#region IF Events
 		void ifKit_Attach(object sender, AttachEventArgs e)	
		{
			Console.WriteLine("Attach");
			initialized = true;
		}
		void ifKit_Detach(object sender, DetachEventArgs e)
		{
			Console.WriteLine("Detach");
		}

		void ifKit_Error(object sender, ErrorEventArgs e)
        {
            //Phidget phid = (Phidget)sender;
            
			//if (tmmp)
			//	throw new PhidgetException(e.Description,(int)e.Code);
			
			if (ErrorOccured != null)
				ErrorOccured(this, e);
			
            switch (e.Type)
            {
                case PhidgetException.ErrorType.PHIDGET_ERREVENT_PACKETLOST:
					
					Console.WriteLine("Packet_Lost");
                    //Ignore this error - it's not useful in this context.
                    return;
                default:
				
					//Console.WriteLine("Phidget exception: ");
					//Console.WriteLine (e.Description);
				break;
                    //throw new ApplicationException(e.Description);
            }
           
        }
		
       	void ifKit_InputChange(object sender, InputChangeEventArgs e)
        {
			if (DigitalInputChanged != null)
				DigitalInputChanged(this, e);
           //Console.WriteLine("Input changed: " + e.Index + " value: " + e.Value);

        }

        //Digital output change event handler
        //Here we check or uncheck the corresponding output checkbox
        //based on the index of the output that generated the event
        void ifKit_OutputChange(object sender, OutputChangeEventArgs e)
        {
			//Console.WriteLine("Output changed: " + e.Index + " value: " + e.Value);
        }

        //Sensor input change event handler
        //Set the textbox content based on the input index that is communicating
        //with the interface kit
        void ifKit_SensorChange(object sender, SensorChangeEventArgs e)
        {
			//Console.WriteLine(ifKit.
			//Console.WriteLine("Sensor changed: " + e.Index + " value: " + e.Value);
			if (AnalogueInputChanged != null)
				AnalogueInputChanged(this, e);
        }		
		#endregion
		
		#region IO Commands
		public void SetDigitalOutput(int outputIndex, bool on)
		{
			
			if (!Ready)
				throw new ApplicationException("IO Board not attached");
			
			if (outputIndex > ifKit.outputs.Count || outputIndex < 0)
				throw new IndexOutOfRangeException("outputIndex is out of bounds");
			
			 ifKit.outputs[outputIndex] = on;
		}
		
		public void SetSensitivity (int outputIndex, int sensorValue)
		{
			if (!Ready)
				throw new ApplicationException("IO Board not attached");
			
			if (outputIndex > ifKit.outputs.Count || outputIndex < 0)
				throw new IndexOutOfRangeException("outputIndex is out of bounds");
			
			ifKit.sensors[outputIndex].Sensitivity = sensorValue;
		}
		
		public int GetSensorValue (int outputIndex)
		{
			if (!Ready)
				throw new ApplicationException("IO Board not attached");
			
			return ifKit.sensors[0].Value;
		}
		
		//use this to protect slots from being occupied by other devices... 
		public void OccupySlot (IIODevice device, IOPortTypes portType)
		{
			throw new NotImplementedException();
		}
		
		#endregion
		

	}
}
