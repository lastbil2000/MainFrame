using System;
using System.Threading;
using MainFrame.Devices;
using MainFrame.Devices.Speech;
using MainFrame.Sensors;
using System.Collections.Generic;
using System.Linq;
using MainFrame.Communication.Log;
using MainFrame.Communication;
using MainFrame.Processes;

namespace MainFrame
{

	public class Robot : IDevice, IRequestable
	{
		
		private static readonly int PROCESS_STARTUP_PAUSE = 100;
		private LinkedList<DeviceContainer> _devices;
		private string _mediatorIdentifier;
		private bool _isReady;
		
		public T Request<T> (INervousSignal<T> message) 
		{
			if (message != null) 
			{
				if(  message is RunSignal &&
			    !((RunSignal)message).Data)
			    	Stop();
				else if (message is ProcessStartSignal)
				{
					ProcessStartSignal signal = message as ProcessStartSignal;
					if (signal.Data.Process != null) {
						
						AddDevice(signal.Data.Identifier, signal.Data.Process);
						StartDevice(signal.Data.Identifier);
					}
					else
						StartDevice(signal.Data.Identifier);
				}	
				else if (message is ProcessStopSignal)
				{
					ProcessStopSignal signal = message as ProcessStopSignal;
					StopDevice(signal.Data.Identifier);
				}
				else if (message is AddDeviceSignal)
				{
					AddDeviceSignal signal = message as  AddDeviceSignal;
					AddDevice(signal.Data.Identifier, signal.Data.Device);
					signal.Data.Device.Start();
				}
				else if (message is GetSignal<Robot>) 
				{
					return (T)(object)this;
				}
				
				
			}
			return default(T);
		}
		
		public Robot (INervousSystem mediator, string mediatorIdentifier = null)
		{
			_devices = new LinkedList<DeviceContainer>();			
			_mediatorIdentifier = mediatorIdentifier;
			mediator.Register(this, "robot"); // <- for now... how to register / retrieve?
		}
		
		private INervousSystem Mediator
		{
			get 
			{
				return (INervousSystem) GetDevice<INervousSystem>(_mediatorIdentifier);
			}
		}
		
		//returns an untyped object (process or device) if found
		public IDevice Get (string identifier) 
		{
			if (!_devices.Contains(new DeviceContainer() {Identifier = identifier}))
				return default(IDevice);
			
			return _devices.Find( new DeviceContainer() {Identifier = identifier}).Value.Device;
		}
		
		public void SetMediator (INervousSystem mediator)
		{
			AddDevice(_mediatorIdentifier, mediator);
		}
		
		public System.Collections.Generic.IEnumerable<Type> Subjects {
			get {
				yield return typeof(RunSignal);
				yield return typeof(ProcessStopSignal);
				yield return typeof(ProcessStartSignal);
				yield return typeof(AddDeviceSignal);
				yield return typeof(GetSignal<Robot>);
			}
		}
		
		private void StopDevice (IDevice device) 
		{
			IRequestable requestable = device as IRequestable;
			
			if (requestable != null &&
				Mediator.Contains(requestable))	
				Mediator.Unregister(requestable);
			
			IProcess process = device as IProcess;
			
			if (process != null &&
				!process.IsRunning)
				StaticLogger.w("Can not stop Process: " + device.ToString() + " is not running.");
			else
				device.Stop();
		}
		
		public void StopDevice (string identifier) 
		{
			
			if (!_devices.Contains(new DeviceContainer() {Identifier = identifier}))
				throw new ApplicationException("Unabe to stop device: Device identifier not found " + identifier);
			
			IDevice device = _devices.Find( new DeviceContainer() {Identifier = identifier}).Value.Device;
			
			StopDevice(device);

		}

		private void StartDevice (IDevice device) 
		{
			IRequestable requestable = device as IRequestable;
			
			if (requestable != null &&
				!Mediator.Contains(requestable))	
				Mediator.Register(requestable);			
			
			IProcess process = device as IProcess;
			
			if (process != null) {
				if (process.IsRunning)
					StaticLogger.w("Process allready running: " + process.ToString());
				else {
					ThreadPool.QueueUserWorkItem( delegate (object obj) { process.Start(); });
					if ((process as IWaitFor) != null)
						(process as IWaitFor).WaitFor();
				}
			}
			else
				device.Start();
		}
		
		public void StartDevice (string identifier) 
		{
			
			if (!_devices.Contains(new DeviceContainer() {Identifier = identifier}))
				throw new ApplicationException("Unable to start device: Device identifier not found " + identifier);
			
			IDevice device = _devices.Find( new DeviceContainer() {Identifier = identifier}).Value.Device;
			
			StartDevice(device);

		}		
		

		public void AddDevice(string identifier, IDevice device)
		{
			if (_devices.Contains(new DeviceContainer() {Identifier = identifier}))
				throw new ApplicationException("Device identifier added: " + identifier);

			_devices.AddLast(new DeviceContainer() {Identifier = identifier, Device = device });
		}
		
		public T GetDevice<T>(string identifier)
		{
			if (string.IsNullOrEmpty(identifier))
				throw new NullReferenceException("No identifier specified");
			
			if (_devices.Contains(new DeviceContainer() {Identifier = identifier}) )
				return  (T) _devices.Find( new DeviceContainer() {Identifier = identifier}).Value.Device;
			else
				StaticLogger.w("Warning: device with id: " + identifier + " is not found");
			return default(T);
				
		}

		public void Start()
		{
			_isReady = true;
			var deviceEnumerator = _devices.GetEnumerator();
			while (deviceEnumerator.MoveNext())
			{
				
				//if (deviceEnumerator.Current.Device as IWaitFor != null)
				//	(deviceEnumerator.Current.Device as IWaitFor).WaitFor();
				/*
				IProcess process = deviceEnumerator.Current.Device as IProcess;
				
				if (process != null &&
					!process.IsRunning) //This is a process that is not running:
				{
					
					StaticLogger.d("starting process " + deviceEnumerator.Current.Identifier);
					StartDevice(deviceEnumerator.Current.Device);
					while (!deviceEnumerator.Current.Device.Ready)
						Thread.Sleep(PROCESS_STARTUP_PAUSE);
				}
				else if (process == null)  //This is a device:
				{
					
					StaticLogger.d("starting device " + deviceEnumerator.Current.Identifier);
					StartDevice(deviceEnumerator.Current.Device);
				}
				*/
				Console.WriteLine("Starting device: " + deviceEnumerator.Current.Identifier);
				StartDevice (deviceEnumerator.Current.Device);
					
			}
			
			//StaticLogger.d("Attaching all processes...");
			
			while (_isReady) 
			{
				Thread.Sleep(100);
				/*
				var processEnumerator = processes.GetEnumerator();
				while (processEnumerator.MoveNext())
				{
					
					if (!processEnumerator.Current.Process.IsRunning)
						ThreadPool.QueueUserWorkItem(processEnumerator.Current.Process.Run);
					
					//Just to make sure the processes gets the time they need...
					Thread.Sleep(20);
				}
				*/
				
			}
			

		}
		
		public void Stop ()
		{
			/*
			var processEnumerator = _processes.GetEnumerator();
			while (processEnumerator.MoveNext()) 
			{
				StaticLogger.d("stopping process: " + processEnumerator.Current.Identifier);
				processEnumerator.Current.Process.Stop();
			}
			*/
			Console.WriteLine("NUMBE OF DEVICES: " +   _devices.Reverse().Count() + " " +   _devices.Count);
			var enumerator = _devices.Reverse().GetEnumerator();
			int i = 0;
			while (enumerator.MoveNext()) 
			{
				
				StaticLogger.d("[" + i++ + "] stopping device: " + enumerator.Current.Identifier);		    
				StopDevice(enumerator.Current.Device);
			}
			
			_isReady = false;
		}
		
		public bool Ready {get {return _isReady;} }
		
		
	}
}
