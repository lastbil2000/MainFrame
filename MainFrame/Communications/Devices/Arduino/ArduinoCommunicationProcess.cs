using System.Threading;
using System;
using System.Linq;
using System.IO.Ports;
using System.IO;
using MainFrame.Core.Logger;

namespace MainFrame.Devices
{


	public class ArduinoCommunicationProcess : ISerialCommunicationProcess
	{
		//setup variables:
		public static readonly int[] BAUD_RATES = {300, 1200, 2400, 9600, 14400, 19200, 28800, 38400, 57600, 115200};
		private static readonly string BASE_PORT_NAME = "/dev/ttyACM";
		public static readonly int DEFAULT_BAUD_RATE = 9600;
		public static readonly int READ_WRITE_TIMEOUT = 200;
		public static readonly int READ_BUFFER_SIZE = 128;
		
		//intervall after each transfer
		private static int LISTEN_SLEEP_INTERVAL = 110;
		//the arduino will check that these packages are located at the end of a message to make the communication less error prone
		private static readonly byte[] CORRUPTION_CHECK_BYTES = {0xAA};

		//the flags initiating each message:
		//private static int DATA_MESSAGE_FLAG = 0;		
		private static readonly int ERROR_MESSAGE_FLAG = 0x1;		
		private static readonly int WARNING_MESSAGE_FLAG = 0x2;
		private static readonly int OPERATION_COMPLETE_MESSAGE_FLAG = 0x4;
		
		//locking transfer read and transfer writes
		static readonly object serialLock = new object();
		
		//the name of the port to use (under Linux/Ubuntu it's /dev/ttyACM{x}).
		private string _portName;
		//the baud rate for the transfer
		private int _baudRate;
		//is true if the arduino is in a process of reading or writing data
		private bool _isTransfering;
		//is true if the arduino should continue to listen on the serial port from time to time
		private bool _isListening;
		private bool _isBooting;
		//the serial port object used for communication
		private System.IO.Ports.SerialPort _serialPort;

		public int ReadInterval
		{
			get
			{
				return LISTEN_SLEEP_INTERVAL;
			}
		}

		public ArduinoCommunicationProcess (string portName, int baudRate)
		{
			SerialInit(portName, baudRate);
		}

		
		public ArduinoCommunicationProcess ()
		{
			_isBooting = true;
			string detectedPortName = GetFirstPortName();
			if (detectedPortName != null)
				SerialInit(detectedPortName, DEFAULT_BAUD_RATE);
			else
				throw new DeviceException("Unable to locate serial port for search path: " + BASE_PORT_NAME + "*");
		}
		
		private string GetFirstPortName () 
		{
			for (int i = 0; i < 10; i++) 
			{
				if (File.Exists(BASE_PORT_NAME + i.ToString()))
					return BASE_PORT_NAME + i.ToString();
			}
			
			return null;
		}
		
		private void SerialInit (string portName, int baudRate) 
		{
			this._portName = portName;
			
			if (!(from b in BAUD_RATES where b == baudRate select b).Any() )
				throw new ArgumentException("baud rate: " + baudRate + " is out of range. (Please have a look at ArduinoServo.BaudRates...)");
			
			_baudRate = baudRate;
			
			System.ComponentModel.IContainer components = new System.ComponentModel.Container();
			_serialPort = new System.IO.Ports.SerialPort(components);
			//serialPort = new System.IO.Ports.SerialPort();
			
			
			//_serialPort.ErrorReceived += new SerialErrorReceivedEventHandler (serial_ErrorReceived);
			
		    _serialPort.PortName = this._portName;
		    _serialPort.BaudRate = this._baudRate;
			_serialPort.ReadTimeout = READ_WRITE_TIMEOUT;
			_serialPort.WriteTimeout = READ_WRITE_TIMEOUT;
			_serialPort.DtrEnable = true;
			_serialPort.RtsEnable = true;
			_serialPort.Parity = Parity.None;
			_serialPort.DataBits = 8;
			_serialPort.StopBits = StopBits.One;
			_serialPort.Handshake = Handshake.None;
			_serialPort.ReadBufferSize = READ_BUFFER_SIZE;		
		}
		
		//todo: try-find-right serial device driver
		
		~ArduinoCommunicationProcess() 
		{
			Stop();
			//serialPort.DataReceived -= serial_DataReceived;
			_serialPort.ErrorReceived -= serial_ErrorReceived;		
		}
		
		public void Start ()
		{

			try
			{
				if (_serialPort.IsOpen)
					throw new DeviceException("Serial port: " + _portName + " already opened");
				
				_serialPort.Open();
				if (!_serialPort.IsOpen)
					throw new DeviceException("Unable to open serial port: " + _portName);
				
				//It seems like it needs two seconds...

				_isListening = true;
				_isBooting = false;
				SerialDataListen();
				

			}
			catch (Exception ex)
			{
				_isListening = false;
				Log.x(ex);
				
			}
			
		}
		
		
		public void Stop ()
		{
			_isListening = false;
			
			while (_isTransfering)
				Thread.Sleep (LISTEN_SLEEP_INTERVAL);
			
			if (_serialPort != null)
				_serialPort.Close();
		}
		
		public bool Ready 
		{
			get 
			{
				if (_serialPort == null ||
					!_serialPort.IsOpen)
					return false;
				
				if (_isTransfering)
					Thread.Sleep(LISTEN_SLEEP_INTERVAL);
				
				return !_isTransfering && !_isBooting;
			}
		}
		
		public bool IsRunning
		{
			get 
			{
				return _serialPort.IsOpen && !_isBooting;
			}
		}


		private byte[] SerialWriteAndFetchResult (byte [] bytes) 
		{
			
			//TODO: this is just a temporary soultion to handle serial-write hang ups...
			Thread writeThread = new Thread(delegate () {_serialPort.Write (bytes,0,bytes.Length);});
			writeThread.IsBackground = true;
			writeThread.Start();
			writeThread.Join(1000);
			
			if (writeThread.IsAlive) {
				writeThread.Abort();
				Log.e("UNABLE TO SEND BYTES TO SERIAL PORT. Aborting and returning null, with a potentially dangerous outcome. The therad might still be alive out there...");
				return default(byte[]);
			}
			
			Thread.Sleep(LISTEN_SLEEP_INTERVAL);
			byte [] returnData = default(byte[]);
			
			if (_serialPort.BytesToRead > 0) 
			{
				
				int bytesToRead = _serialPort.BytesToRead;
				returnData = new byte[bytesToRead];
				int bytesRead = _serialPort.Read (returnData, 0, bytesToRead);
				
				if (bytesRead != bytesToRead) 
				{
					//Strange:
					Log.e("ARDUINO TANSFER ERROR: Expected " + bytesToRead + " bytes, but got: " + bytesRead);
				}
				else if (returnData[bytesRead - 1] != OPERATION_COMPLETE_MESSAGE_FLAG)
				{
					//Operation must end with PERATION_COMPLETE_MESSAGE_FLAG
					Log.e("ARDUINO TANSFER ERROR: Unexpected end byte: " + (int)returnData[bytesRead - 1] );
				}
				else if (bytesRead > 1)
				{
					//skip the PERATION_COMPLETE_MESSAGE_FLAG in response:
					byte [] tempData =  new byte[bytesRead - 1];
					Buffer.BlockCopy(returnData, 0, tempData, 0, bytesRead - 1);
					returnData = tempData;
				}
			}
			
			//Well... just in case
			_serialPort.DiscardInBuffer();
			
			return returnData;
		}
		
		public byte[] TransferBytes(byte [] bytes) 
		{
			_isTransfering = true;
			
			byte[] readBytes = default(byte[]);

			lock (serialLock) 
			{
				
				if (_serialPort == null)
					throw new NullReferenceException("Serial port was null");
				if (!_serialPort.IsOpen)
					throw new ApplicationException("Serial port is not open");
				
				if (!_serialPort.BaseStream.CanWrite)
					Log.e("Can't write to port!");
				
			/*	Console.Write("Writing ");
				
				foreach (byte b in serialData)
					Console.Write("{0} ", (int) b);
				Console.WriteLine(" to arduino.");
			*/
				
				readBytes = SerialWriteAndFetchResult(Concatenate(bytes, CORRUPTION_CHECK_BYTES));
			}
			
			_isTransfering = false;
			
			return readBytes;
		}
		
		public byte[] TransferData (ISerialPackage data)
		{
			if (data == null)
				throw new NullReferenceException("ISerialPackage was null");
			
			return TransferBytes (Concatenate(data.Header, data.Data));
		}
		
		private byte[] Concatenate (params byte[][] bytes) {
			int size = 0;
			
			for (int i = 0; i < bytes.Length; i++) 
				size += bytes[i].Length;
			
			byte[] data = new byte[size];
			
			int j = 0;
			for (int i = 0; i < bytes.Length; i++)
			{
				
				foreach (byte b in bytes[i])
					data[j++] = b;
			}
			
			return data;
		}
		
		private void SerialDataListen() 
		{
			while (_isListening) 
			{
				while(_isTransfering)
					Thread.Sleep(LISTEN_SLEEP_INTERVAL);
				
				if (!_isListening)
					return;
				
				
				
				lock (serialLock) 
				{
					_isTransfering = true;
					//wait for bytes to be transfered
					if (_serialPort.BytesToRead > 0)
						Thread.Sleep(LISTEN_SLEEP_INTERVAL / 2);
					else 
					{
						_isTransfering = false;
						continue;
					}
					int statusByte = _serialPort.ReadByte();
					if (statusByte != 0) 
					{
						string message = "";
						if (_serialPort.BytesToRead > 0)
						{
							if (statusByte == ERROR_MESSAGE_FLAG)
							{
								message = _serialPort.ReadExisting();
								Log.e("__serial: " + message);
							}
							else if (statusByte == WARNING_MESSAGE_FLAG)
							{
								message = _serialPort.ReadExisting();
								Log.w("__serial: " + message);
							}
							else 
							{
								message = _serialPort.ReadExisting();
								Log.d("__serial: " + new String((char) statusByte,1) + message);
							}
						}
						else
							Log.w("__serial: No bytes to read.");
						
						
					}
					else
					{
						if (_serialPort.BytesToRead > 0)
						{
							
							byte [] serialData = new byte[_serialPort.BytesToRead];
							_serialPort.Read(serialData,0,_serialPort.BytesToRead);
							string logMessage = "__serial bytes: ";
							foreach (byte b in serialData)
								logMessage  += "[" + (int)b + "] ";
							
							Log.d(logMessage);
							
						}
						else
							Log.w("__serial warning: no bytes to read.");
					}
					
				}
				//potential thread conflicts here?
				_isTransfering = false;
				
			}
		}
		
		private void serial_ErrorReceived(object sender,  SerialErrorReceivedEventArgs e) 
		{
			Log.e("Serial communication error: " + e.EventType.ToString());
		}
				
		public void WaitFor (int milliSeconds = 1000)
		{
			
			while (_isBooting)
				Thread.Sleep(100);
			
			bool lockTaken = false;

			try
			{
				Monitor.TryEnter(serialLock, milliSeconds, ref lockTaken);
			}
			finally 
			{ 
				if (lockTaken) 
					Monitor.Exit (serialLock); 
			}
			
		}
	
	}
}
