using MainFrame.Devices;
using MainFrame.Processes;
using MainFrame.Communication;
using MainFrame.Devices.Speech;
using System.Collections.Generic;
using System;

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
			//robot.StartDevice("com");
			
			//arduino.WaitFor();
			
			robot.AddDevice ("pinCom", DeviceFactory.GetArduinoPinCom(robot.GetDevice<ISerialCommunicationProcess>("com")));
			
			robot.AddDevice ("servoBoard", DeviceFactory.GetPin(
				robot.GetDevice<IPinCom>("pinCom"), 0xA));

			internalsDone = true;
		}
		
		
		public void BuildArm ()
		{
			if (armDone)
				throw new ApplicationException("Arm already created");

			
			if (!internalsDone)
				throw new ApplicationException("Internals NOT created");

			robot.AddDevice("phidgetServo", DeviceFactory.GetServo(0));
			/*
			IServo psTest = robot.GetDevice<IServo>("phidgetServo");
			
			psTest.Start();
			while (!psTest.Ready)
				System.Threading.Thread.Sleep(100);
			psTest.Rotation = 95;
			*/
			robot.AddDevice("arduinoServo1", DeviceFactory.GetServo(robot.GetDevice<ISerialCommunicationProcess>("com"), 0xB, 0));
			robot.AddDevice("arduinoServo2", DeviceFactory.GetServo(robot.GetDevice<ISerialCommunicationProcess>("com"), 0xC, 2));
			robot.AddDevice("arm", DeviceFactory.GetArm(
				robot.GetDevice<IServo>("phidgetServo"),
				robot.GetDevice<IServo>("arduinoServo1"),
				robot.GetDevice<IServo>("arduinoServo2")));

			armDone = true;
		}
		
		
		public void BuildHead ()
		{
			if (!internalsDone)
				throw new ApplicationException("Internals NOT created");
			
			robot.AddDevice("arduinoServoHeadX", DeviceFactory.GetServo(robot.GetDevice<ISerialCommunicationProcess>("com"), 0xC, 0));
			robot.AddDevice("arduinoServoHeadY", DeviceFactory.GetServo(robot.GetDevice<ISerialCommunicationProcess>("com"), 0xC, 1));			
			
			robot.AddDevice ("head", DeviceFactory.GetHead(
				robot.GetDevice<IServo>("arduinoServoHeadX"),
				robot.GetDevice<IServo>("arduinoServoHeadY")));
			
			robot.AddDevice ("laser", DeviceFactory.GetPin(
				robot.GetDevice<IPinCom>("pinCom"), 0x9));

			robot.AddDevice("video0", ProcessFactory.GetVideo(0));
			robot.StartDevice("video0");
			
			robot.AddDevice("ir_meter", DeviceFactory.GetArduinoIRDistanceMeter(
				robot.GetDevice<ISerialCommunicationProcess>("com")
				));			
			
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
		}
		
		public void BuildSensors () 
		{
			robot.AddDevice("sonar1", DeviceFactory.GetSRF05Sonar(robot.GetDevice<ISerialCommunicationProcess>("com")));
			robot.AddDevice("sonar2", DeviceFactory.GetEZ1Sonar(robot.GetDevice<ISerialCommunicationProcess>("com")));
		}
		
		public void BuidEngine() 
		{
			var servo = DeviceFactory.GetServo (robot.GetDevice<ISerialCommunicationProcess>("com"), 0xB, 2);
			var pin = DeviceFactory.GetPin (robot.GetDevice<IPinCom>("pinCom"), 0x8);
			robot.AddDevice("engine_power", pin);
			robot.AddDevice("engine_servo", servo);
			robot.AddDevice("engine", DeviceFactory.GetEngine(servo, pin));
		}
		
		
	}
}
