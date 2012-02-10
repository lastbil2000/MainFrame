using MainFrame.Devices;
using MainFrame.Processes;
using MainFrame.Communication;
using MainFrame.Devices.Speech;
using System.Collections.Generic;
using System;
using MainFrame.Devices.Video;
namespace MainFrame
{


	public class StaticRobotBuilder : IRobotBuilder
	{
		bool internalsDone;
		bool armDone;
		Robot robot;
		DeviceFactory DeviceFactory {
			get 
			{
				return robot.GetDevice<DeviceFactory>("device_factory");
			}
		}
		ProcessFactory ProcessFactory {
			get 
			{
				return robot.GetDevice<ProcessFactory>("process_factory");
			}
		}
		
		public StaticRobotBuilder ()
		{
			
			DeviceFactory factory = new DeviceFactory();
			

			robot = new Robot(
				factory.GetMediator(),
				"mediator"
				);
			
			robot.AddDevice("device_factory", factory);
			robot.AddDevice("process_factory", new ProcessFactory());
			
			robot.AddDevice("dummy", new DummyDevice());
			
		}

		public Robot GetRobot ()
		{
			return robot;
		}
		
		public void BuildInternals ()
		{
			if (internalsDone)
				throw new ApplicationException("Internals already created");
			
			var arduino = ProcessFactory.GetArduino();
			robot.AddDevice("com", arduino);
			robot.StartDevice("com");
			
			//arduino.WaitFor();
			
			robot.AddDevice ("pinCom", DeviceFactory.GetArduinoPinCom(robot.GetDevice<ISerialCommunicationProcess>("com")));
			
			robot.AddDevice ("servo_board", DeviceFactory.GetPin(
				robot.GetDevice<IPinCom>("pinCom"), 0xA));

			internalsDone = true;
		}
		
		
		public void BuildArm ()
		{
			if (armDone)
				throw new ApplicationException("Arm already created");

			
			if (!internalsDone)
				throw new ApplicationException("Internals NOT created");

			robot.AddDevice("phidget_servo", DeviceFactory.GetServo(0));
			robot.AddDevice("s1", DeviceFactory.GetServo(robot.GetDevice<ISerialCommunicationProcess>("com"), 0xC, 0));
			robot.AddDevice("s2", DeviceFactory.GetServo(robot.GetDevice<ISerialCommunicationProcess>("com"), 0xC, 1));
			robot.AddDevice("s3", DeviceFactory.GetServo(robot.GetDevice<ISerialCommunicationProcess>("com"), 0xC, 2));
			robot.AddDevice("s4", DeviceFactory.GetServo(robot.GetDevice<ISerialCommunicationProcess>("com"), 0xB, 0));			
			robot.AddDevice("s5", DeviceFactory.GetServo(robot.GetDevice<ISerialCommunicationProcess>("com"), 0xB, 1));
			robot.AddDevice("s6", DeviceFactory.GetServo(robot.GetDevice<ISerialCommunicationProcess>("com"), 0xB, 2));
			
			robot.AddDevice("hand_servo", DeviceFactory.GetServo(robot.GetDevice<ISerialCommunicationProcess>("com"), 0xB, 0));
			robot.AddDevice("rotation_servo", DeviceFactory.GetServo(robot.GetDevice<ISerialCommunicationProcess>("com"), 0xB, 1));
			robot.AddDevice("arm", DeviceFactory.GetArm(
				robot.GetDevice<IServo>("phidget_servo"),
				robot.GetDevice<IServo>("rotation_servo"),
				robot.GetDevice<IServo>("hand_servo")));
			
			armDone = true;
		}
		
		
		public void BuildHead ()
		{
			if (!internalsDone)
				throw new ApplicationException("Internals NOT created");
			
			robot.AddDevice("head_x", DeviceFactory.GetServo(robot.GetDevice<ISerialCommunicationProcess>("com"), 0xC, 1));
			robot.AddDevice("head_y", DeviceFactory.GetServo(robot.GetDevice<ISerialCommunicationProcess>("com"), 0xC, 0));			
			
			robot.AddDevice ("head", DeviceFactory.GetHead(
				robot.GetDevice<IServo>("head_x"),
				robot.GetDevice<IServo>("head_y")));
			
			robot.AddDevice ("laser", DeviceFactory.GetPin(
				robot.GetDevice<IPinCom>("pinCom"), 0x9));

			//robot.AddDevice("video0", ProcessFactory.GetVideo(0));
			//robot.StartDevice("video0");	
			
		}
		
		public void BuildSpeech () 
		{
			robot.AddDevice("sphinx", ProcessFactory.GetASR());
			robot.StartDevice("sphinx");
			robot.AddDevice("voice", DeviceFactory.GetVoice());
			//robot.StartDevice("voice");
			robot.AddDevice("command_interpreter", DeviceFactory.GetDevice<CommandInterpreter>(MainClass.LanguageFile));
			robot.AddDevice("speech_interpreter", DeviceFactory.GetDevice<SpeechInterpreter>(MainClass.LanguageFile));
			
				
			//IDistance GetArduinoIRDistanceMeter (ISerialCommunicationProcess com, int arduinoPort = 1)	
		}
		
		public void AttachProcesses() 
		{
			//robot.AddDevice("keyboard", ProcessFactory.GetKeyboardProcess());
			//robot.StartDevice("keyboard");
			
			//robot.AddDevice("look",
			//	new LookAtPeopleProcess(
			//	robot.GetDevice<MainFrame.Devices.Video.ICamera>("video0"), 
			//	robot.GetDevice<MainFrame.Devices.IHead>("head")));
			
			List<string>  paths = new List<string>();
			paths.Add("/usr/local/ironruby/lib/ruby/site_ruby/1.9.1");
			paths.Add("/usr/local/ironruby/lib/ruby/1.9.1");
			paths.Add("/usr/local/ironruby/lib/ironruby");
			paths.Add("/usr/bin");
			paths.Add("/usr/lib/ruby/1.9.1/x86_64-linux");
			paths.Add("/usr/lib/ruby/1.9.1");

			robot.AddDevice("keyboard", ProcessFactory.GetRuby("scripts/keyboard.rb", 
				robot.GetDevice<INervousSystem>("mediator"), paths));
			
			robot.StartDevice("keyboard");
			
			
			ICamera camera = robot.GetDevice<ICamera>("video0");
						IVideoCapture faceObject = new DeviceFactory().GetHaarCapture(camera, "haarcascade_frontalface_alt.xml");
						robot.AddDevice ("face_obj", faceObject);
						faceObject.Start();
						
						robot.AddDevice("face_look",
							new ProcessFactory().GetLookAtPeopleProcess(
								robot.GetDevice<IVideoCapture>("face_obj"),
								robot.GetDevice<IHead>("head")
							));
		}
		
		public void BuildSensors () 
		{
			robot.AddDevice("sonar1", DeviceFactory.GetSRF05Sonar(robot.GetDevice<ISerialCommunicationProcess>("com")));
			robot.AddDevice("sonar2", DeviceFactory.GetEZ1Sonar(robot.GetDevice<ISerialCommunicationProcess>("com")));
			robot.AddDevice("ir", DeviceFactory.GetArduinoIRDistanceMeter(robot.GetDevice<ISerialCommunicationProcess>("com")));
			robot.AddDevice("battery", DeviceFactory.GetBattery());
		}
		
		public void BuidEngine() 
		{
			var servo = DeviceFactory.GetServo (robot.GetDevice<ISerialCommunicationProcess>("com"), 0xC, 2);
			var pin = DeviceFactory.GetPin (robot.GetDevice<IPinCom>("pinCom"), 0x8);
			robot.AddDevice("engine_power", pin);
			robot.AddDevice("engine_servo", servo);
			robot.AddDevice("engine", DeviceFactory.GetEngine(servo, pin));
		}
		
		
	}
}
