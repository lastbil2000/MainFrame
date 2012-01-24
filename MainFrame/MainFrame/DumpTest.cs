using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using ProtoBuf;
using ProtoBuf.ServiceModel;
using System.Threading;
using MainFrame.Speech;
using MainFrame.Communication;
using MainFrame.Devices;
using System.Diagnostics;
using Phidgets;
using System.Collections.Generic;
using System.Linq;
using MainFrame.Sensors;
using System.IO.Ports;
using MainFrame.Communication.Log;
using MainFrame.Devices.Video;
#pragma warning disable

namespace MainFrame
{

	/*
	public class Testish : ITriggerable
	{

		public void TriggerOnEvent (object sender, EventArgs e)
		{
			DTOReceivedEventArgs evt = (DTOReceivedEventArgs)e;
			
			if (evt.newDTO is TestDTO)
				Console.WriteLine("hej pappa: " + ((TestDTO)evt.newDTO).TestVal);
			
			((ISocketConnection)sender).Respond(new TestDTO() {TestVal= (Convert.ToInt32(((TestDTO)evt.newDTO).TestVal) + 1).ToString() });
		}

	}

	*/
	[ProtoContract]
	public class ProtoTest
	{
		[ProtoMember (1)]
		public string TestVal {get;set;}	
		
	}
	
	public static class DumpTest
	{
		/*
		public static void transfer (uint data, int count)
		{
			for (int serialBit = 0; serialBit < count; serialBit++)
			{
				board.SetDigitalOutput(0, (data & 1) == 1);
				data = (data >> 1);
				TestServo.Delayx(delay);
			}			
		}
		public static uint InvertBits (uint data)
		{
			return InvertBits(data, 8);
		}
			
		public static uint InvertBits (uint data, int count)
		{
			uint ret = 0;
			
			for (int serialBit = 0; serialBit < count; serialBit++)
			{
				ret += (uint)(  (data & 1) << 
					Convert.ToUInt16((count - 1) - serialBit));
				data = (data >> 1);
			}
			
			return ret;
		}		
			/*
		public IEnumerable<T> Request<T>(T message)
		{
			Console.WriteLine ("mehh----------------");
			
		
			List<INervousSignal<T>> result = new List<INervousSignal<T>>();
			
			bool found = false;
			foreach(IRequestable client in clients)
				foreach (Type subject in client.Subjects)
					if (subject.Equals(message.Subject))
					{
						found = true;
						//yield return client.Request<T>(message);
						result.Add (client.Request<T>(message));
					}
			
			
			return result;
			
			return null;
			/*
			if (found)
				return false;
				//return new List<INervousSignal<T>>();
			else
				yield break;
			*/
			
		//}
		public static bool isRunning = true;
		/*
		
		public static void RuntimeMethodHandle ()
		{
			
			testTCP();
			testTCPServer();
			testTCPSocket();
		}
		
		public static void testTCPSocket2()
		{
			
			IPEndPoint ip = new IPEndPoint(IPAddress.Any, 4242);
			
			Socket socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
			
			socket.Bind(ip);
			socket.Listen(10);
			socket.Blocking = true;
			
			
			while (true)
			{
				Socket client = socket.Accept();
				IPEndPoint newclient = (IPEndPoint)client.RemoteEndPoint;
				Console.WriteLine("Connected with {0} at port {1}",newclient.Address, newclient.Port);
				
				NetworkStream ns = new NetworkStream(client);

				const int kHeaderSize = 6;
				const int kHeaderDescriptorSize = 2;
				
				byte [] header = new  byte[kHeaderDescriptorSize];
				ns.Read(header,0,kHeaderDescriptorSize);
				byte [] headerDTOSize = new  byte[sizeof(uint)];
				ns.Read(headerDTOSize,0,sizeof(uint));
				uint DTOSize = BitConverter.ToUInt32(headerDTOSize,0);
				byte [] body = new byte[DTOSize];
				ns.Read(body, 0,(int)DTOSize);
				ProtoTest test = Serializer.Deserialize<ProtoTest>(new MemoryStream(body));

				Console.WriteLine(" " + test.TestVal);
				
				Console.WriteLine("Sending...");
				
				Thread.Sleep(10);		

				header = new byte[kHeaderSize];
				header[0] = 67; header[1] = 68;

				MemoryStream DTOWriteStream = new MemoryStream();
				test.TestVal = "Hello Iphone";
				Serializer.Serialize<ProtoTest>(DTOWriteStream, test);
				DTOSize = (uint)DTOWriteStream.Length;
				for (int oo = 0; oo < sizeof(uint); oo++)
					header[oo + kHeaderDescriptorSize] = BitConverter.GetBytes(DTOSize)[oo];
				ns.Write(header,0,kHeaderSize);
				ns.Write(DTOWriteStream.GetBuffer(),0,(int)DTOSize);
				
				ns.Flush();
				ns.Close(); 
			}
		}
		
		public static void testTCPSocket()
		{
			 ProtoTest pObj = new ProtoTest(){TestVal = "Hello Iphone!"};
			
		      Int32 port = 4243;
		      TcpListener server = new TcpListener(port);

		      server.Start();
			Socket socket;
			int i = 0;
		      while(isRunning) 
		      {
		        Console.Write("Waiting for a connection... ");
		        
				
				if (i++ > 3) 
					isRunning = false;
				
				socket = server.AcceptSocket();
				Console.Write("established!");
				socket.Blocking = true;
				byte []header = new byte[2];
				
				using (NetworkStream stream = new NetworkStream(socket)) {
					
					stream.Read(header,0,2);
					Console.WriteLine("header: " + header[0]);
					Console.WriteLine("de-serializing");
					pObj = Serializer.Deserialize<ProtoTest>(stream);
					socket = server.AcceptSocket();
					stream.Close();
					Console.WriteLine(pObj.TestVal);
					//stream.Flush();
					//
					
				}
				
				//socket.Close();
				//socket = server.AcceptSocket();
				
				using (NetworkStream stream = new NetworkStream(socket)) {

					pObj.TestVal = "Hello iPhone" + i;
					Console.WriteLine("sending reply");
					Serializer.Serialize<ProtoTest>(stream, pObj);
					stream.Flush();
				}
				
				
				socket.Close();
			}
			
			server.Stop();
			return;

			
		}
		
		
		public static void testTCPServer()
		{

			 ProtoTest pObj = new ProtoTest(){TestVal = "Hello Iphone!"};
			
		      // Set the TcpListener on port 13000.
		      Int32 port = 4243;
		      //IPAddress localAddr = IPAddress.Parse("127.0.0.1");
		      
		      // TcpListener server = new TcpListener(port);
		      TcpListener server = new TcpListener(port);
		
		      // Start listening for client requests.
		      server.Start();
		         
		      // Buffer for reading data
		      Byte[] bytes = new Byte[10];
		      //String data = null;
				TcpClient client;
			// Enter the listening loop.
				int i = 0;
		      while(isRunning) 
		      {
		        Console.Write("Waiting for a connection... ");
		        pObj.TestVal = "Hello iPhone" + i;
				
				if (i++ > 3) 
					isRunning = false;
		        // Perform a blocking call to accept requests.
		        // You could also user server.AcceptSocket() here.
		        
				client = server.AcceptTcpClient();
				
		        Console.WriteLine("Connected!");

		        // Get a stream object for reading and writing
		        NetworkStream stream = client.GetStream();
				
				if (stream.CanRead)
				{
				    Console.WriteLine("ptbf de-serialize");             
					pObj = ProtoBuf.Serializer.Deserialize<ProtoTest>(stream);
					Console.WriteLine(pObj.TestVal);
					
				}
				else
					Console.WriteLine("cannot read");
				client.Close();
				client = server.AcceptTcpClient();
				if (stream.CanWrite)
				{
					ProtoBuf.Serializer.Serialize<ProtoTest>(stream,pObj);
					Console.WriteLine("ptbf serialize");
			        // Loop to receive all the data sent by the client.
					//stream.WriteByte(42);
					//stream.
					stream.Flush();
					Console.WriteLine("ptbf flush1");
				}
				else
					Console.WriteLine("cannot write");
				//client = server.AcceptTcpClient();
				
				//Console.WriteLine(pObj.TestVal);
				//stream.Flush();
				
//		        while((i = stream.Read(bytes, 0, bytes.Length))!=0) 
//		        {   
//		          // Translate data bytes to a ASCII string.
//		          data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
//		          Console.WriteLine(String.Format("Received: {0}", data));
//		       
//		          // Process the data sent by the client.
//		          data = data.ToUpper();
//		
//		          byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
//		
//		          // Send back a response.
//		          stream.Write(msg, 0, msg.Length);
//		          Console.WriteLine(String.Format("Sent: {0}", data));            
//		        }
		        
		        // Shutdown and end connection
		        client.Close();
				//server.Stop();
				//return;
		      }
		        
			server.Stop();
		  }   
		
		static void testTCP()
		{
			 string address = "www.axel-wilhelm.se";
		    int port = 80;
		
		    TcpClient client = new TcpClient(address,port);
		    NetworkStream stream = client.GetStream( );
		
		    //StreamWriter writer = new StreamWriter(stream);
		
			ProtoTest t = new ProtoTest() {TestVal = "hej lilla pappa"};
			Console.WriteLine("serializing");
			Serializer.Serialize<ProtoTest>(stream, t);
			
			stream.Flush();
			Console.WriteLine("stream flushed");
		    //writer.WriteLine("www.axel-wilhelm.se\n");
		    //writer.Flush( );
		
		    using (StreamReader reader = new StreamReader(stream)) {
		      while (reader.Peek( ) != -1) {
		        Console.WriteLine(reader.ReadLine( ));
		      }
		    }			
		}
		*/
	
		public static bool shouldCaptureFace;
		public static bool shouldRunFaceRunner;
		
		public static void faceRunner() {
			
			CameraProcess video = new CameraProcess();
			//video.Start();
			IVideoCapture capture = video.CreateHaarCapture("haarcascade_frontalface_alt.xml");
			shouldRunFaceRunner = true;
			while (shouldRunFaceRunner) {
				if (shouldCaptureFace) {
					Console.WriteLine("try capture: " + capture.Capture());
					shouldCaptureFace = false;
				}
				Thread.Sleep(100);
			}
			
			video.Stop();
			
		}
		
		public static void Main2 (string[] args) 
		{
			StaticLogger.Instance.AddLogger(new ConsoleLogger());
			INervousSystem nervs = new RobotNervous();
			
			ConsoleKeyInfo kkk = new ConsoleKeyInfo('a',ConsoleKey.A, false, false, false);
			
			ITTS voice = new EspeakTTS();
			voice.SetMediator(nervs);
			
//			SphinxASR asr = new Speech.SphinxASR(
//				"language.arpa",
//				"language.dict",
//				"hub4wsj_sc_8k"
//			);
			
			//asr.SetMediator (nervs);
			//nervs.Register(asr);
			nervs.Register(voice);
			//Thread tt = new Thread(SphinxASRinstanceHolder.ASR.Start);
			//tt.IsBackground = true;
			//tt.Start();
			
			kkk = Console.ReadKey();
			//asr.Stop();
			return;
			
			ISerialCommunicationProcess comProvider = new ArduinoCommunicationProcess();
			//refactor: comProvider.Start();
			IServo arduinoS = new ArduinoServo(comProvider,0x0C,0);
			/*
			for (int i = 0; i < 8; i++)
			{
				while (!arduinoS.Ready)
					Thread.Sleep(1000);
				//Thread.Sleep(1000);
				arduinoS.Rotation = i;
			}
			1 1000 00110010
			*/
			int rot = 42;
			while (kkk.Key != ConsoleKey.Escape) 
			{
				while (!arduinoS.Ready)
					Thread.Sleep(200);

				
				
				kkk = Console.ReadKey();
				
				if (kkk.Key == ConsoleKey.LeftArrow)
					rot -= 10;
				else if (kkk.Key == ConsoleKey.RightArrow)
					rot +=10;
				
				if (rot < 0) rot =0;
				if (rot > 180) rot = 180;
				arduinoS.Rotation = rot;
			}
			comProvider.Stop();
		}
		
		public static bool delmeStop;
		public static void readKeyThread(object data) 
		{
			readKeyThreadRun = true;
			ConsoleKeyInfo kkk = new ConsoleKeyInfo('a',ConsoleKey.A, false, false, false);
			while (kkk.Key != ConsoleKey.Escape) {
				
				kkk = Console.ReadKey();
				Console.Write(kkk.KeyChar);
			}
			delmeStop = true;
			Console.WriteLine("stop");
			readKeyThreadRun = false;
		}
		public static bool readKeyThreadRun;
		
		public static bool intensiveWorkThreadRun;
		public static void intensiveWorkThread(object data) 
		{
			Console.WriteLine("Begin: 1");
			intensiveWorkThreadRun = true;
				for (int xx = 0; xx < 1000000000;xx++)  if (xx == 999) Console.Write(1);
				//int a =  0; for (int xx = 0; xx < 1000000;xx++) for (int xxy = 0; xxy < 1000000;xxy++) for (int xxz = 0; xxz < 1000000;xxz++) { if (a > 1000) a = 0; a++;}
			intensiveWorkThreadRun = false;
		}
		
		public static bool intensiveWorkThreadRun2;
		public static void intensiveWorkThread2(object data) 
		{
			Console.WriteLine("Begin: 2");
			intensiveWorkThreadRun2 = true;
			for (int xx = 0; xx < 1000000000;xx++)  if (xx == 9999) Console.Write(2);
				//int a =  0; for (int xx = 0; xx < 1000000;xx++) for (int xxy = 0; xxy < 1000000;xxy++) for (int xxz = 0; xxz < 1000000;xxz++) { if (a > 1000) a = 0; a++;}
			intensiveWorkThreadRun2 = false;
		}
		public static bool intensiveWorkThreadRun3;
		public static void intensiveWorkThread3(object data) 
		{
			Console.WriteLine("Begin: 3");
			intensiveWorkThreadRun3 = true;
				for (int xx = 0; xx < 1000000000;xx++)  if (xx == 99999) Console.Write(3);
				//int a =  0; for (int xx = 0; xx < 1000000;xx++) for (int xxy = 0; xxy < 1000000;xxy++) for (int xxz = 0; xxz < 1000000;xxz++) { if (a > 1000) a = 0; a++;}
			intensiveWorkThreadRun3 = false;
		}
		public static bool intensiveWorkThreadRun4;
		public static void intensiveWorkThread4(object data) 
		{
			Console.WriteLine("Begin: 4");
			intensiveWorkThreadRun4 = true;
				for (int xx = 0; xx < 1000000000;xx++)  if (xx == 9999999) Console.Write(4);
				//int a =  0; for (int xx = 0; xx < 1000000;xx++) for (int xxy = 0; xxy < 1000000;xxy++) for (int xxz = 0; xxz < 1000000;xxz++) { if (a > 1000) a = 0; a++;}
			intensiveWorkThreadRun4 = false;
		}
		public static bool intensiveWorkThreadRun5;
		public static void intensiveWorkThread5(object data) 
		{
			Console.WriteLine("Begin: 5");
			intensiveWorkThreadRun5 = true;
				for (int xx = 0; xx < 1000000000;xx++)  if (xx == 9999999) Console.Write(5);
				//int a =  0; for (int xx = 0; xx < 1000000;xx++) for (int xxy = 0; xxy < 1000000;xxy++) for (int xxz = 0; xxz < 1000000;xxz++) { if (a > 1000) a = 0; a++;}
			intensiveWorkThreadRun5 = false;
		}

		public static void Main5 (string[] args)
		{
			//ThreadPool.q
			//Thread t = new Thread(delme2);
			//t.IsBackground = true;
			//t.Start();
			
			while (!delmeStop) {
				if (!intensiveWorkThreadRun)
					ThreadPool.QueueUserWorkItem(intensiveWorkThread);
								if (!intensiveWorkThreadRun2)
					ThreadPool.QueueUserWorkItem(intensiveWorkThread2);
								if (!intensiveWorkThreadRun3)
					ThreadPool.QueueUserWorkItem(intensiveWorkThread3);
								if (!intensiveWorkThreadRun4)
					ThreadPool.QueueUserWorkItem(intensiveWorkThread4);
								if (!intensiveWorkThreadRun5)
					ThreadPool.QueueUserWorkItem(intensiveWorkThread5);
				if (!readKeyThreadRun)
					ThreadPool.QueueUserWorkItem(readKeyThread);
			}
			
			return;
			ConsoleKeyInfo kkk = new ConsoleKeyInfo('a',ConsoleKey.A, false, false, false);
			StaticLogger.Instance.AddLogger(new ConsoleLogger());
			CameraProcess v = new CameraProcess();
			//v.Start();
			IVideoCapture capture = v.CreateHaarCapture("haarcascade_frontalface_alt.xml");
			
			while (kkk.Key != ConsoleKey.Escape) {
				
				kkk = Console.ReadKey();
				if (capture.Capture()) {
					Console.WriteLine("Capture succeeded!");
					capture.Image.Save("face.png");
					Mono.Unix.Native.Stdlib.system("scp face.png olga@lastbil2000.dyndns.org:/home/olga/Projects/sthlm.server/public/");
				}
				
			}
			v.Stop();
			
		}
		
		
		
		public static void Main3 (string[] args)
		{
			StaticLogger.Instance.AddLogger(new ConsoleLogger());

			ConsoleKeyInfo kkk = new ConsoleKeyInfo('a',ConsoleKey.A, false, false, false);
			
			ISerialCommunicationProcess comProvider = new ArduinoCommunicationProcess();
			//refactor: comProvider.Start();

			INervousSystem nervs = new RobotNervous();

			ITTS voice = new EspeakTTS();
			voice.SetMediator(nervs);
			nervs.Register(voice);
			Console.WriteLine("jag finns");
			
			//kkk = Console.ReadKey();
	
			
			//return;
			//LkTest lktest = new LkTest(nervs);
			//return;

			Console.WriteLine("starting arm...");
			IServo armservo =   new PhidgetServo(0); //new ArduinoServo(comProvider,0x0C,0); 
			
			armservo.Start();
			
			while (!armservo.Ready) Thread.Sleep(100);
			
			//faceDetector.Startx();
			
			Console.WriteLine("running");

			
			BatteryLevelSensor battery = new BatteryLevelSensor();
			
			//IComUnit comUnit = new PhidgetComUnit(board, 0,1);
			IServo servo1 = new ArduinoServo(comProvider,0x0B,0);
				//new PhidgetComServo (comUnit,1,0);
			IServo servo2 = new ArduinoServo(comProvider,0x0B,1);
				// new PhidgetComServo (comUnit,0,2);
			
			//armservo.Rotation = 90;
			servo1.Rotation = 90;
			Thread.Sleep(100);
			servo2.Rotation = 90;
			Thread.Sleep(100);
			Arm3Engines arm = new Arm3Engines(armservo, servo2, servo1);
			
			
		
			nervs.Register(battery);
			nervs.Register(arm);
			
			//FaceDetector faceDetector = new FaceDetector();
			//faceDetector.Start();
			
			Thread.Sleep(5000);
			Thread faceThread = new Thread(faceRunner);
			faceThread.IsBackground = true;
			faceThread.Start();
			
			/*
			List<Arm3Engines.Arm3EnginesRequest> positions = new List<Arm3Engines.Arm3EnginesRequest>();
			positions.Add(new Arm3Engines.Arm3EnginesRequest(90, 90, true));
			positions.Add(new Arm3Engines.Arm3EnginesRequest(60, 60, false));
			positions.Add(new Arm3Engines.Arm3EnginesRequest(90, 120, true));
			positions.Add(new Arm3Engines.Arm3EnginesRequest(110, 100, false));
			//nervs.Request<
			
			while (true) {
				foreach (Arm3Engines.Arm3EnginesRequest position in positions)
				{
					nervs.Request(position);
					Thread.Sleep(1000);
				}
			}
			*/
			int rot = 90;
			
			int up = 90;
			
			while (kkk.Key != ConsoleKey.Escape)
			{
				kkk = Console.ReadKey();
				/*
				if (kkk.Key == ConsoleKey.T)
				{
				
					nervs.Request(new SpeechSensor.SpeechSignal()
					{
						Subject = typeof(SpeechSensor.SpeechSignal),
						Data = "test"
					});
					
					int level = nervs.Request<int>(new BatteryLevelSensor.BatteryLevelRequest());
					Console.WriteLine("PAPPA" + level);
					
				}
				*/
				
				
					if (kkk.Key == ConsoleKey.LeftArrow ||
				    kkk.Key == ConsoleKey.RightArrow)
				{
					if (kkk.Key == ConsoleKey.LeftArrow)
						rot-=10;
					else if (kkk.Key == ConsoleKey.RightArrow)
						rot+=10;
					
					if (rot < 0)
						rot = 0;
					if (rot > 180)
						rot = 180;
					
					arm.Rotate = rot;
				}

				else if (kkk.Key == ConsoleKey.UpArrow ||
				    kkk.Key == ConsoleKey.DownArrow)
				{
					if (kkk.Key == ConsoleKey.DownArrow)
						up-=10;
					else if (kkk.Key == ConsoleKey.UpArrow)
						up+=10;
					
					if (up < 0)
						up = 0;
					if (up > 180)
						up = 180;					
					while (!servo2.Ready)
						Thread.Sleep(10);
					
					arm.Up = up;
				}
				else if (kkk.Key == ConsoleKey.O)
					arm.Open = true;
				else if (kkk.Key == ConsoleKey.A)
					arm.Open = false;
				
				else if (kkk.Key == ConsoleKey.Enter)
					shouldCaptureFace = true;
			}
			
			
			//faceDetector.Stop();
			armservo.Stop();
			comProvider.Stop();
			/*
			if (faceDetector != null &&
			    faceDetector.IsRunning)
			faceDetector.Stop();
			*/
			Console.WriteLine("-------------------------------------------");
			//throw new ApplicationException("DIE!");
			return;
			

			while (kkk.Key != ConsoleKey.Escape)
			{
				
				kkk = Console.ReadKey();
				
			}
			
						
			Console.WriteLine("japp");
			//Console.WriteLine("\n\n\n''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''TestServo(20000, 500, 5000);");
			//
			//Console.WriteLine("\n\n\n''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''TestServo(40000, 1000, 5000);");
			//TestServo t2 = new TestServo(40000, 1000, 500);
			//Console.WriteLine("\n\n\n''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''TestServo(10000, 250, 5000);");
			//TestServo t3 = new TestServo(10000, 250, 500);
			
			return;
			
			/*
			SocketServer<DTOConnection> server = new SocketServer<DTOConnection>(4242);
			Testish tetet = new Testish();
			server.AddTriggerable(tetet);
			server.Start();
			
			//DumpTest.testTCPSocket2();
			Console.WriteLine("running...");
			
			//Console.ReadKey();
			while (Console.ReadKey().Key != ConsoleKey.Escape)
				Thread.Sleep(100);
			Console.WriteLine("Stopping...");
			//string apa = Console.ReadLine();
			server.Stop();
			Console.WriteLine("DONE " + new MainFrame.Sensors.BatteryLevel().GetLevel() + " " + sizeof(uint));
			*/
			return;
				
			/*
			ProtoTest t = new ProtoTest() {TestVal = "hej lilla pappa"};
			Console.WriteLine("serializing");
			Serializer.Serialize<ProtoTest>(System.Console.OpenStandardOutput(), t);
			
			System.Console.OpenStandardOutput().Flush();
			*/
			//TcpListener server = new TcpListener(4242);
			//server.Start();
			
			//DumpTest.testTCPSocket2();
			//testTCP();
			Robot r = new Robot();
			r.Start();
			Console.WriteLine("...ready and running... press any key to exit");
			Console.ReadKey();
			r.Stop();
			/**/
			
			
			
		}
	}
	
	
}

/*
      //Parses command line arguments and calls the appropriate open
        #region Command line open functions
        private void openCmdLine(Phidget p)
        {
            openCmdLine(p, null);
        }
        private void openCmdLine(Phidget p, String pass)
        {
            int serial = -1;
            int port = 5001;
            String host = null;
            bool remote = false, remoteIP = false;
            string[] args = Environment.GetCommandLineArgs();
            String appName = args[0];

            try
            { //Parse the flags
                for (int i = 1; i < args.Length; i++)
                {
                    if (args[i].StartsWith("-"))
                        switch (args[i].Remove(0, 1).ToLower())
                        {
                            case "n":
                                serial = int.Parse(args[++i]);
                                break;
                            case "r":
                                remote = true;
                                break;
                            case "s":
                                remote = true;
                                host = args[++i];
                                break;
                            case "p":
                                pass = args[++i];
                                break;
                            case "i":
                                remoteIP = true;
                                host = args[++i];
                                if (host.Contains(":"))
                                {
                                    port = int.Parse(host.Split(':')[1]);
                                    host = host.Split(':')[0];
                                }
                                break;
                            default:
                                goto usage;
                        }
                    else
                        goto usage;
                }

                if (remoteIP)
                    p.open(serial, host, port, pass);
                else if (remote)
                    p.open(serial, host, pass);
                else
                    p.open(serial);
				
                return; //success
            }
            catch { }
        usage:
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Invalid Command line arguments." + Environment.NewLine);
            sb.AppendLine("Usage: " + appName + " [Flags...]");
            sb.AppendLine("Flags:\t-n   serialNumber\tSerial Number, omit for any serial");
            sb.AppendLine("\t-r\t\tOpen remotely");
            sb.AppendLine("\t-s   serverID\tServer ID, omit for any server");
            sb.AppendLine("\t-i   ipAddress:port\tIp Address and Port. Port is optional, defaults to 5001");
            sb.AppendLine("\t-p   password\tPassword, omit for no password" + Environment.NewLine);
            sb.AppendLine("Examples: ");
            sb.AppendLine(appName + " -n 50098");
            sb.AppendLine(appName + " -r");
            sb.AppendLine(appName + " -s myphidgetserver");
            sb.AppendLine(appName + " -n 45670 -i 127.0.0.1:5001 -p paswrd");
			
			throw new ApplicationException( sb.ToString());
        }
        #endregion
		
 */


		
/*
		private static System.IO.Ports.SerialPort serialPort1;
		
		private static void OnReceived(object sender, SerialDataReceivedEventArgs c)
		{
		    int ch;
		    try
		    {
		        // write out text coming back from the arduino
		        Console.Write(serialPort1.ReadExisting());
		    }
		    catch (Exception exc) { }
		}
		
		private static void serialTest () 
		{
			System.ComponentModel.IContainer components = new System.ComponentModel.Container();
			
			//return;
		    serialPort1 = new System.IO.Ports.SerialPort(components);
			
			
		    serialPort1.PortName = "/dev/ttyACM0";
		    serialPort1.BaudRate = 9600;
			
		    serialPort1.Open();
			if (!serialPort1.IsOpen)
		    {
		        Console.WriteLine("Oops");
		        return;
		    }
		
		    // this turns on !
		    //serialPort1.DtrEnable = true;
		
		    // callback for text coming back from the arduino
		    //serialPort1.DataReceived += OnReceived;
		
		    // give it 2 secs to start up the sketch
		    System.Threading.Thread.Sleep(2000);
		
		    using (serialPort1)
		    {
	            Console.WriteLine("sending green command");
	            serialPort1.Write(new byte[] { 99 } , 0, 1);
				//serialPort1.WriteLine("d");
	            System.Threading.Thread.Sleep(1000);
				
	            //Console.WriteLine("sending red command");
	            //serialPort1.Write(new byte[] { 103 }, 0, 1);
				
	            //System.Threading.Thread.Sleep(5000);
		    }
		}
 
*/
#pragma warning restore