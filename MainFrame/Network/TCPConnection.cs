using System;
using System.Net.Sockets;
using System.Net;
using MainFrame.Core.Logger;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using MainFrame.Core.Network.Extensions;

namespace MainFrame.Core.Network
{
	internal class TCPConnection : IByteArrayConnection
	{
		private Socket _socket;
		private bool _isRunning;
		private IPEndPoint _remoteIpEndPoint;
		private NetworkStream _networkStream;
		private IDataReceived<byte []> _observer;
		private bool _busy;
		
		private readonly byte [] PACKAGE_CONTROL = {0xFF, 0, 0xFF, 0, 0xFF, 0, 0xFF, 0};
		private const int DEFAULT_BUFFER_SIZE = 4096;
		
		private Object _lock = new Object();
		
		
		public TCPConnection (TcpClient client, IDataReceived<byte []> observer)
		{
			if (client == null)
				throw new NullReferenceException ("Client must not be null.");
			
			if (!client.Connected)
				throw new ApplicationException ("Client not connected!");
				
			_socket = client.Client;
			_isRunning = true;
			_observer = observer;
			
			_remoteIpEndPoint = _socket.RemoteEndPoint as IPEndPoint;
			
			Log.d("Connection established: " + IP, this.ToString());
		
		}
		
		public bool Ready 
		{
			get 
			{
				return _networkStream != null &&
					   _socket.Connected;
			}
		}
		
		public bool Busy 
		{
			get 
			{
				return _busy;
			}
		}
		
		public string IP 
		{
			get 
			{
				return _remoteIpEndPoint.Address + ":" + _remoteIpEndPoint.Port;
			}
		}
				
		
		public void Start() {
			
			while (_isRunning) {
				using (_networkStream = new NetworkStream(_socket)) {
					
					//Log.d ("Waiting read...");
					WaitForData(_networkStream, ref _isRunning);
					//Log.d ("...done waiting");
					
					_busy = true;
					lock (_lock) 
					{
						
						//Log.d("About to get from: " + IP);
						
						if (!_isRunning)
							return;
						byte [] controlHeader = ReadStream (_networkStream, PACKAGE_CONTROL.Length);
						if (!CompareArrays (controlHeader ,PACKAGE_CONTROL))
							throw new ApplicationException ("Control header error for: " + IP);
						
						if (!_isRunning)
							return;
						int contentLength = BitConverter.ToInt32(ReadStream (_networkStream, sizeof (int)),0);
						
						if (contentLength < 1)
							throw new ApplicationException("Content length in header was: " + contentLength);
						
						
						if (!_isRunning)
							return;
						byte []buffer = ReadStream (_networkStream,contentLength);
						
						/*
						Log.d ("Read response write:");
						_networkStream.Write(PACKAGE_CONTROL,0, PACKAGE_CONTROL.Length);
						_networkStream.Flush();
						*/
						_observer.DataReceived(buffer, IP);
						//Log.d("done read");
						
					}
					_busy = false;
					
				}
			}
			
			Log.d ("Connection finished.");
			
			
			
		}
		
		private void WaitForData (NetworkStream stream, ref bool condition) 
		{
			
			while (!stream.DataAvailable) 
			{
				if (!condition) {
					//Log.d ("WILL DIE!");
						return;
				}
				
				
				Thread.Sleep(100);
			}	
				
				
		}
		
		private byte [] ReadStream (NetworkStream stream, int expectedSize = -1) {
			
			//Log.d ("ReadStream: " + expectedSize);
			byte [] tmpBuffer = new byte [DEFAULT_BUFFER_SIZE < expectedSize ? 
				DEFAULT_BUFFER_SIZE : expectedSize];
			
			int size = 0;
			int bytesRead = 0;
			IList<byte[]> chunks = new List<byte[]>();
			
			while (size < expectedSize)
			{
				int readSize = expectedSize - size < tmpBuffer.Length ?
							   expectedSize - size : tmpBuffer.Length;
				
				bytesRead = stream.Read(tmpBuffer, 0, readSize);
				
				if (bytesRead > 0) 
				{
					byte [] tmp = new byte[bytesRead];
					System.Array.Copy(tmpBuffer,tmp, bytesRead);
					chunks.Add(tmp);
					size += bytesRead;
				}
			}
			
			byte []buffer = new byte[size];
			int offset = 0;
			foreach (byte [] chunk in chunks) 
			{
				System.Array.Copy(chunk,0, buffer, offset, chunk.Length);
				
				offset += chunk.Length;
			}
					
			return buffer;
			
		}
		
		private bool CompareArrays (byte [] array1, byte [] array2) 
		{
			if (array1 == null && array2 == null)
				return true;
			
			if (array1 == null || array2 == null)
				return false;
			
			if (array1.Length != array2.Length)
				return false;
			
			for (int i = 0; i < array1.Length; i++)
				if (array1[i] != array2[i])
					return false;
			
			return true;
			
		}
		
		
		
		public void Stop () 
		{
			lock (_lock) 
			{
				_isRunning = false;
			}
			
			
			//_client.EndConnect();
		}
		
		public void Write (byte [] buffer) 
		{
			
			if (buffer == null)
				throw new NullReferenceException ("Buffer cannot be null!");
			
			if (buffer.Length == 0) 
				throw new ArgumentException ("Length of buffer must be > 0");
			
			if (_busy)
				Log.w("Warning, resource is busy: " + IP);
			//	throw new ApplicationException ("Unable to write. Is busy: " + IP);
			
			_busy = true;
			lock (_lock) 
			{
				while (!_networkStream.CanWrite) 
				{
					Log.w ("Cannot write to stream: " + IP);
					Thread.Sleep(100);
				} 
				
				if (_isRunning)  
				{
				//Log.d("writing...");
				byte [] size = BitConverter.GetBytes(buffer.Length);
				
				byte []writeBuffer = new byte[size.Length + PACKAGE_CONTROL.Length + buffer.Length];
				
				//_networkStream.Write(PACKAGE_CONTROL,0, PACKAGE_CONTROL.Length);
				//_networkStream.Write(size,0, size.Length);
				//_networkStream.Write(buffer,0, buffer.Length);
				System.Array.Copy(PACKAGE_CONTROL,0,writeBuffer,0,PACKAGE_CONTROL.Length);
				System.Array.Copy(size,0,writeBuffer,PACKAGE_CONTROL.Length,size.Length);
				System.Array.Copy(buffer,0,writeBuffer,PACKAGE_CONTROL.Length + size.Length,buffer.Length);
				_networkStream.Write(writeBuffer,0,writeBuffer.Length);
				_networkStream.Flush();
				writeBuffer = null;
				//Log.d ("wrote");
				/*
				Log.d ("Waiting for write verification");
				
				WaitForData (_networkStream, ref _isRunning);
				
				if (_isRunning) 
				{
					byte []controlHeader = ReadStream(_networkStream, PACKAGE_CONTROL.Length);
					
					if (!CompareArrays (controlHeader ,PACKAGE_CONTROL))
						throw new ApplicationException ("Write verification: Control bytes failed for: " + IP);

				}
				
				Log.d("got write verification: done writing");
				*/

				}
			}
			_busy = false;
		}
	}
}

