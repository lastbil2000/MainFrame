using System;
using MainFrame.Devices.Speech;
using MainFrame.Communication;
using System.Threading;
using MainFrame.Devices;
using System.Diagnostics;
using Phidgets;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;
using MainFrame.Core.Logger;
using MainFrame.Devices.Video;
using System.Xml;
using System.Xml.XPath;
using MainFrame.Core.Network;

namespace MainFrame
{
	class MainClass
	{
		
		class Observer : IClientConnection {
			public string IP;
			#region IClientConnection implementation
			public void ClientConnected (string ip)
			{
				IP = ip;
			}
	
			public void ClientDisconnected (string ip)
			{
				throw new System.NotImplementedException ();
			}
			#endregion
		}
		
		
		
		private static Observer observer;
		public static void Main (string[] args)  
		{
			
			Log.Instance.AddLogger(new ConsoleLogger());	
			Log.ShowThreadId = true;
			
			observer = new Observer();
			
			Server s = new Server();
			
			s.AddObserver(observer);
			s.Start();
			
			Thread.Sleep(1000);
			IClient c = new Client();
			c.Connect("127.0.0.1", Server.DEFAULT_PORT);
			Thread.Sleep(1000);
			
			while (!c.Ready) {
				Console.Write("x");
				Thread.Yield();
			}
			//c.Write(new byte [16]);
			//c.Write(new byte[65536 * 4 + 1024]);
			
			for (int i = 0; i < 100; i++) 
			{
				//c.Write(new byte[3]);
				while (c.Busy)
					Thread.Sleep(100);
				
				c.Write(new byte[65536 * 4 + 1024]);
				//System.Threading.Thread.Sleep(1000);
			//Console.WriteLine ("hej");
				while (s.IsBusy(observer.IP))
					Thread.Sleep(100);
				
				s.Write(new byte[65536 * 4 + 1024], observer.IP);

			}
			Thread.Sleep(1000);
			while (c.Busy)
				Thread.Sleep(100);
			
			c.Disconnect();
			//Console.WriteLine ("nestan");
			s.Stop();
			
			Console.WriteLine ("-----------FINISH-----------------");
		}
		
		public static void Main2 (string[] args) 
		{
			ISerialCommunicationProcess com = new ArduinoCommunicationProcess();
			                                                                  
			Thread t = new Thread(com.Start);
			t.Start();
			com.WaitFor();
			Console.WriteLine("tryyyck ");
			Console.ReadKey();
			IPinCom pinCom = new DeviceFactory().GetArduinoPinCom(com);
			pinCom.SetPin(0xA, true);
			com.WaitFor();
			
			IServo s  = new ArduinoServo(com,0xC, 0);
			IServo s2  = new ArduinoServo(com,0xC, 1);
			
			s.Rotation = 90;
			s2.Rotation = 90;
			
			Console.ReadKey();
			com.Stop();
			
		}
		
		public static string BaseDir {get; private set;}
		public static string ScriptDirectory {get; private set;}
		public static string TargetDir {get; private set;}
		public static string LanguageFile {get; private set;}
		
		public static void Main3 (string[] args) 
		{
			if (args.Length < 1) {
				BaseDir = "/home/olga/workspace/robot";
				
				TargetDir = "MainFrame/MainFrame/bin/Debug";
				ScriptDirectory =  BaseDir + "/script";
				//throw new Exception ("MainFrame <base dir> [<target dir>] [<script dir>]");
			}
			else
			{
				BaseDir = args[0];
			
				if (args.Length < 2)
					TargetDir = BaseDir;
				else
					TargetDir = args[1];
	
				if (args.Length < 3)
					ScriptDirectory = BaseDir;
				else
					ScriptDirectory = BaseDir + "/" + args[2];
			}
			LanguageFile = BaseDir + "/resources/language.xml";
			//Console.WriteLine(MainClass.ScriptDirectory + "/sphinx/create_language_files.sh " + MainClass.BaseDir + " language " + MainClass.TargetDir);
			//return;
			
			Log.Instance.AddLogger(new ConsoleLogger());	
			///sbin/wpa_supplicant -B -dd -D wext -i wlan0 -t -W -c/etc/wpa_supplicant/wpa_supplicant.conf
			IRobotBuilder builder = new StaticRobotBuilder();
			
			builder.BuildInternals();
			builder.AttachProcesses();
			builder.BuildSensors();
			builder.BuildArm();
			builder.BuildHead();
			builder.BuidEngine();
			builder.BuildSpeech();
			
			//Thread.Sleep(2000);
			
			
			Robot r = builder.GetRobot();
			Robot.BaseDir = BaseDir;
			Robot.ScriptDirectory = ScriptDirectory;
			Robot.TargetDir = TargetDir;
			Robot.LanguageFile = LanguageFile;
			
			IPin servoBoard = r.GetDevice<IPin>("servo_board");
			
			//while (!servoBoard.Ready) Thread.Sleep (10);
			//Thread.Sleep(2000); 
			servoBoard.On = true;
			
			Console.WriteLine("Starting...");
			r.Start();
			
			
			
			//Console.ReadKey();
			//r.Stop();
			
		}
	}
}
