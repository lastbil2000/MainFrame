using System;
using System.Linq;
using System.IO.Ports;

namespace MainFrame.Devices
{


	public class ArduinoComUnit : IComUnit
	{
		
		private System.IO.Ports.SerialPort serialPort;
		private string portName;
		private int baudRate;
		public static int[] BaudRates = {300, 1200, 2400, 9600, 14400, 19200, 28800, 38400, 57600, 115200};
		public static byte arduinoDeviceId = 1;
		
		//arduino will check that these packages are located at the end of a message to make the communication less error prone
		public static byte[] corruptionCheckBytes = {0xAA};
		public void TransferData (IComUnitPackage data)
		{
			if (serialPort == null)
				throw new NullReferenceException("Serial port was null");
			if (!serialPort.IsOpen)
				throw new ApplicationException("Serial port is not open");
			
			byte [] serialData = new byte[/*4 + corruptionCheckBytes.Length*/ 50];
			
			serialData[0] = (byte)arduinoDeviceId;
			serialData[1] = (byte)data.OutPort;
			serialData[2] = (byte)data.Header;
			serialData[3] = (byte)data.Data;
			
			
			for (int i = 0; i < corruptionCheckBytes.Length; i++);
				//serialData[serialData.Length + i] = 0;//corruptionCheckBytes[i];
			
			serialPort.Write (serialData,0,serialData.Length);
			/*
			serialPort.Write (new byte[] {(byte)arduinoDeviceId},0,1);
			serialPort.Write (new byte[] {(byte)data.OutPort},0,1);
			serialPort.Write (new byte[] {(byte)data.Header},0,1);
			serialPort.Write (new byte[] {(byte)data.Data},0,1);
			*/
			
			
		}
		
		public void setPortName(string portName) 
		{
			this.portName = portName;
		}

		public void setBaudRate(int baudRate) 
		{
			if (!(from b in BaudRates where b == baudRate select b).Any() )
				throw new ArgumentException("baud rate: " + baudRate + " is out of range. (Please have a look at ArduinoComUnit.BaudRates...)");
			
			this.baudRate = baudRate;
		}
		
		public string getPortName() 
		{
			return portName;
		}
		
		public int getBaudRate() 
		{
			return baudRate;
		}

		
		public IIOBoard Board {
			get {
				throw new System.NotImplementedException("Arduino uses serial communications. No IIOBoard required.");
			}
		}
		
		
		public int DataPort {
			get {
				return -1;
			}
		}
		
		
		public int StatusPort {
			get {
				return -1;
			}
		}
		

		public ArduinoComUnit ()
		{
			portName = "/dev/ttyACM0";
			baudRate = 9600;
		}
		
		
		public void Start ()
		{
			System.ComponentModel.IContainer components = new System.ComponentModel.Container();
			serialPort = new System.IO.Ports.SerialPort(components);
			
		    serialPort.PortName = portName;
		    serialPort.BaudRate = baudRate;
			
			serialPort.Open();
			if (!serialPort.IsOpen)
				throw new ApplicationException("Unable to open serial port: " + portName);
			
			//It seems like it needs two seconds...
			System.Threading.Thread.Sleep(2000);
			
		}
		
		
		public void Stop ()
		{
			if (serialPort != null)
				serialPort.Close();
		}
		
		
		public bool Ready {
			get {
				return serialPort.IsOpen;
			}
		}
		
	}
}
