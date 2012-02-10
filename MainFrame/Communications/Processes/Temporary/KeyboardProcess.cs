using MainFrame.Communication.Log;
using MainFrame.Communication;
using System;
using MainFrame;
using MainFrame.Devices;
using MainFrame.Devices.Video;
using System.Threading;
using System.Collections.Generic;

namespace MainFrame.Processes
{


	public class KeyboardProcess : IProcess, IRequestable
	{

		public bool IsRunning {get; private set;}
		private bool shouldRun;
		public string Name {get; private set;}
		
		INervousSystem mediator;
		
		public KeyboardProcess ()
		{
			shouldRun = true;
		}
		public void SetMediator(INervousSystem mediator)
		{
			this.mediator = mediator;
		}
		
		public bool Ready { get {return IsRunning;} }
		
		public void Start() 
		{
			
		try
		{
			if (IsRunning)
				throw new ProcessException("Keyboardprocess already started...");
			
			IsRunning = true;
			ConsoleKeyInfo key = new ConsoleKeyInfo('a',ConsoleKey.A, false, false, false);
			while (shouldRun) {
					
				
				key = Console.ReadKey();
				//Console.Write("KEY: " + key.KeyChar);
				if (key.Key == ConsoleKey.Escape) 
				{
					mediator.Request(new RunSignal() {Data = false, Identifier = "robot"});
					shouldRun = false;
				}
				
				else if (key.Key == ConsoleKey.Q) {
						Robot robot = mediator.Request<Robot>(new GetSignal<Robot>());
						robot.GetDevice<IServoMotor>("engine").Stop();
				}
				else if (key.Key == ConsoleKey.J) {
						Robot robot = mediator.Request<Robot>(new GetSignal<Robot>());
						robot.GetDevice<IServoMotor>("engine").Velocity -= 10;
				}
				
				else if (key.Key == ConsoleKey.K) {
						Robot robot = mediator.Request<Robot>(new GetSignal<Robot>());
						robot.GetDevice<IServoMotor>("engine").Velocity += 10;
				}
				/*
				else if (key.Key == ConsoleKey.X) {
						Robot robot = mediator.Request<Robot>(new GetSignal<Robot>());
						robot.GetDevice<IServoMotor>("leftEngine").Velocity = +10;
				}
				*/
				else if (key.Key == ConsoleKey.B) {
						Robot robot = mediator.Request<Robot>(new GetSignal<Robot>());
						if (robot.GetDevice<IServo>("arduinoServoHeadX").Ready)
							
							robot.GetDevice<IServo>("arduinoServoHeadX").Rotation -= 10;
				}
				else if (key.Key == ConsoleKey.M) {
						Robot robot = mediator.Request<Robot>(new GetSignal<Robot>());
						if (robot.GetDevice<IServo>("arduinoServoHeadX").Ready)
							
							robot.GetDevice<IServo>("arduinoServoHeadX").Rotation += 10;
				}
				else if (key.Key == ConsoleKey.W) {
						Robot robot = mediator.Request<Robot>(new GetSignal<Robot>());
						if (	robot.GetDevice<IServo>("arduinoServoHeadY").Ready)
							robot.GetDevice<IServo>("arduinoServoHeadY").Rotation -= 10;
				}
				else if (key.Key == ConsoleKey.V) {
						Robot robot = mediator.Request<Robot>(new GetSignal<Robot>());
						if (robot.GetDevice<IServo>("arduinoServoHeadY").Ready)
							robot.GetDevice<IServo>("arduinoServoHeadY").Rotation += 10;
				}
				else if (key.Key == ConsoleKey.E) {
						Robot robot = mediator.Request<Robot>(new GetSignal<Robot>());
						var distance = robot.GetDevice<IDistance>("ir_meter");
						Console.WriteLine ("Distance: " + distance.Distance);
				}
				else if (key.Key == ConsoleKey.U) {
						Robot robot = mediator.Request<Robot>(new GetSignal<Robot>());
						var distance = robot.GetDevice<IDistance>("sonar2");
						Console.WriteLine ("Distance: " + distance.Distance);
				}					
				else if (key.Key == ConsoleKey.PageUp) {
						Robot robot = mediator.Request<Robot>(new GetSignal<Robot>());
						IPin pin = robot.GetDevice<IPin>("laser");
						if (pin.Ready)
							pin.On = !pin.On;
				}
				else if (key.Key == ConsoleKey.PageDown) {
						/*
						Robot robot = mediator.Request<Robot>(new GetSignal<Robot>());
						IPinCom pin = robot.GetDevice<IPinCom>("pin");
						if (pin.Ready)
							pin.SetPin(0x0A, false);
						*/
				}
				else if (key.Key == ConsoleKey.F12) 
				{
					
					Robot robot = mediator.Request<Robot>(new GetSignal<Robot>());
					robot.StopDevice("sphinx");
					robot.StopDevice("speech_interpreter");
					string newQ = "xxx";
					string newA = "yyy";
						
					List<string> questions = new List<string>();
					List<string>answers = new List<string>();
						
					while (newQ.Trim() != "") 
					{
						StaticLogger.d ("Enter a new Question (enter to skip)");
						newQ = Console.ReadLine();
						if (newQ.Trim() != "")
							questions.Add(newQ);
					}

					while (newA.Trim() != "") 
					{
						StaticLogger.d ("Enter a new Answers (enter to skip)");
						newA = Console.ReadLine();
						if (newA.Trim() != "")
							answers.Add(newA);
					}
					
					if (questions.Count > 0 && answers.Count > 0) 
					{
						MainFrame.Devices.Speech.LanguageEditor le = new MainFrame.Devices.Speech.LanguageEditor(Robot.LanguageFile);
						le.Insert(questions, answers);
						le.CompileLanguageFiles(	Robot.ScriptDirectory,
							                        Robot.BaseDir,
							                        Robot.TargetDir)
						;
					}
					else
						StaticLogger.w("Not enough questions or answers...");
					
					robot.StartDevice("sphinx");
					robot.StartDevice("speech_interpreter");
					
						
				}
				
					
				else if (key.Key == ConsoleKey.Insert) {
					
					Robot robot = mediator.Request<Robot>(new GetSignal<Robot>());
					if (robot.GetDevice<IVideoCapture>("face_obj") == null) 
					{
						
						ICamera camera = robot.GetDevice<ICamera>("video0");
						IVideoCapture faceObject = new DeviceFactory().GetHaarCapture(camera, "haarcascade_frontalface_alt.xml");
						robot.AddDevice ("face_obj", faceObject);
						faceObject.Start();
						
						robot.AddDevice("face_look",
							new ProcessFactory().GetLookAtPeopleProcess(
								robot.GetDevice<IVideoCapture>("face_obj"),
								robot.GetDevice<IHead>("head")
							));
						robot.StartDevice("face_look");
						StaticLogger.d("EHM");
					}
					else
						StaticLogger.w("Face stuff already added...");
				}

				Arm3Engines arm = mediator.Request<Arm3Engines>(new GetSignal<Arm3Engines>());
				if (arm != null) 
				{
					if (key.Key == ConsoleKey.LeftArrow && arm.Rotation > 10)
						arm.Rotation -= 10;
					else if (key.Key == ConsoleKey.RightArrow && arm.Rotation < 180)
						arm.Rotation += 10;
					else if (key.Key == ConsoleKey.DownArrow && arm.Up > 10)
						arm.Up -=10;
					else if (key.Key == ConsoleKey.UpArrow && arm.Up  < 180)
						arm.Up += 10;
					else if (key.Key == ConsoleKey.O)
						arm.Open = true;
					else if (key.Key == ConsoleKey.A)
						arm.Open = false;
				}
					

			}
			
			IsRunning = false;
		  }
		  catch (Exception ex)
		  {
				StaticLogger.x(ex);
				mediator.Request(new RunSignal() {Data = false, Identifier = "robot"});
				shouldRun = false;

		  }
		}
		
		public void Stop() 
		{
			shouldRun = false;
		}
		
		public T Request<T> (INervousSignal<T> message) 
		{
			if (message != null && 
			    message is RunSignal &&
			    !((RunSignal)message).Data)
			    	Stop();
			
			return (T)(object) IsRunning;
		}
		
		public System.Collections.Generic.IEnumerable<Type> Subjects {
			get {
				yield return typeof(RunSignal);
			}
		}
	}
}
