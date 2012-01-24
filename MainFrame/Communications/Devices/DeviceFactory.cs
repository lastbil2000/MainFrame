using System;
using MainFrame.Devices.Video;
using MainFrame.Devices.Speech;
using MainFrame.Communication;

namespace MainFrame.Devices
{

	public class DeviceFactory : IDevice
	{

		public DeviceFactory ()
		{
		}
		
		public INervousSystem GetMediator() {
			return new RobotNervous();
		}
		
		public IServo GetServo(int phidgetPort) 
		{
			return new PhidgetServo(phidgetPort);
		}
		
		public IServoMotor GetEngine(IServo servo, IPin pin) 
		{
			return new ArduinoEngine(servo, pin);
		}
		
		public IServo GetServo(ISerialCommunicationProcess arduinoProvider, int arduinoPort, int servoPort)
		{
			return new ArduinoServo(arduinoProvider, arduinoPort, servoPort);
		}
		
		public Arm3Engines GetArm (IServo armServo, IServo rotateServo, IServo handServo) 
		{
			return new Arm3Engines(armServo, rotateServo, handServo);
		}
		
		public IHead GetHead (IServo horizontal, IServo vertical) 
		{
			return new Head(horizontal, vertical);
		}
		
		public ITTS GetVoice () 
		{
			return new EspeakTTS();
		}
		
		public IVideoCapture GetHaarCapture(ICamera camera, string haarFile) 
		{
			if (camera == null)
				throw new NullReferenceException("Camera was null!");
			if (!camera.IsRunning)
				throw new MainFrame.Processes.ProcessException("Camera is not running!");
			
			return camera.CreateHaarCapture(haarFile);
		}
		
		public T GetDevice<T> (params object [] args) where T : class, IDevice
		{
			if (typeof (T).Equals(typeof (Speech.CommandInterpreter)))
				return (T)(object) new Speech.CommandInterpreter ((string) args[0]);

			if (typeof (T).Equals(typeof (Speech.SpeechInterpreter)))
				return (T)(object) new Speech.SpeechInterpreter ((string) args[0]);

			return default(T);
		}

		public IPin GetPin (IPinCom com, int index) 
		{
			return new Pin(com, index);
		}

		
		public IPinCom GetArduinoPinCom (ISerialCommunicationProcess com) 
		{
			return new ArduinoPinCom(com);
		}
		
		public IDistance GetArduinoIRDistanceMeter (ISerialCommunicationProcess com, int arduinoPort = 1)
		{
			return new ArduinoIRSensor(com, arduinoPort);
		}
		
		public IDistance GetSRF05Sonar (ISerialCommunicationProcess com, int echoPort = 3, int dataPort = 4) 
		{
			return new ArduinoSRF05Sonar(com);
		}
		
		public IDistance GetEZ1Sonar (ISerialCommunicationProcess com, int pwPort = 5) 
		{
			return new ArduinoEZ1Sonar(com);
		}
		
		public void Start () {}
		public void Stop () {}
		public bool Ready { get { return true; } }
		
		//TODO: handle this in another manner...
		/*
		public SpeechSensor GetEars() 
		{
			return new SpeechSensor("language.xml");
		}
		*/
		
	}
}
