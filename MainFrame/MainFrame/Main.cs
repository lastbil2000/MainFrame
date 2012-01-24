using System;
using MainFrame.Devices.Speech;
using MainFrame.Communication;
using System.Threading;
using MainFrame.Devices;
using System.Diagnostics;
using Phidgets;
using System.Collections.Generic;
using System.Linq;
using MainFrame.Sensors;
using System.IO.Ports;
using MainFrame.Communication.Log;
using MainFrame.Devices.Video;
using System.Xml;
using System.Xml.XPath;

namespace MainFrame
{
	class MainClass
	{
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
		
		public static void Main (string[] args) 
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
			
			StaticLogger.Instance.AddLogger(new ConsoleLogger());	
			///sbin/wpa_supplicant -B -dd -D wext -i wlan0 -t -W -c/etc/wpa_supplicant/wpa_supplicant.conf
			IRobotBuilder builder = new StaticRobotBuilder();
			
			builder.BuildInternals();
			builder.BuildSensors();
			builder.BuildArm();
			builder.BuildHead();
			builder.BuidEngine();
			builder.BuildSpeech();
			//Thread.Sleep(2000);
			builder.AttachProcesses();
			
			Robot r = builder.GetRobot();
			
			/*
			IPin servoBoard = r.GetDevice<IPin>("servoBoard");
			while (!servoBoard.Ready)
				Thread.Sleep (10);
			
			servoBoard.On = true;
			*/
			r.Start();
			
			
			
			//Console.ReadKey();
			//r.Stop();
			
		}
	}
}
