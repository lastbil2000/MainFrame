using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MainFrame.Communication.Net
{

	public class DTOReceivedEventArgs : EventArgs
	{
		public IDTO newDTO {get; set;}
		public int newHeader {get; set;}
	}

	public class DTOConnection : ISocketConnection
	{
		protected Socket client;
		protected IPEndPoint clientEndpoint;
		protected NetworkStream networkStream;
		
		protected bool isWaitingForReply;
		protected IDTO responseDTO;
		
		const int kHeaderSize = 6;
		const int kHeaderDescriptorSize = 2;
		
		public bool IsTransfering{get; protected set;}
		public IPAddress Address {get { return clientEndpoint.Address; } }
		public int Port {get { return clientEndpoint.Port; } }
		
		public event EventHandler DataReceived;
		
		public DTOConverter converter;
		
		public void Reply (IDTO dto)
		{
			
		}
		
		public DTOConnection ()
		{
			converter = new DTOConverter();
			//SetSocket(client);
			
		}
		
		
		public void Start(Socket client)
		{
			if (client == null)
				throw new NullReferenceException("Socket Client must be set. Use SetSocket");

			this.client = client;
			clientEndpoint = (IPEndPoint)client.RemoteEndPoint;			
			
			IsTransfering = true;
			
			using (networkStream = new NetworkStream(client))
			{
				Console.WriteLine("Reading handshake");
				byte [] handShakeHeader = new byte[1];
				networkStream.Read(handShakeHeader,0,1);
				
				while (IsTransfering)
				{
					Console.WriteLine("Waiting for data...");
					while (!networkStream.DataAvailable)
					{
						Thread.Sleep(100);
						if (!networkStream.CanRead)
							return;
					}
					
					isWaitingForReply = true;
					
					if (DataReceived == null)
						throw new ApplicationException("No event handler hooked up for this connection!");
					
					DataReceived(this, new DTOReceivedEventArgs {
						newDTO = readDTO(networkStream)
					});
					
					while (isWaitingForReply)
						Thread.Sleep(10);
					
					Console.WriteLine("Sending...");
					
;					writeDTO(networkStream, responseDTO);
				}
			}
			//ns.Close(); 
			//IsTransfering = false;
			Console.WriteLine("Transfel done...");
		}
		
		private IDTO readDTO(NetworkStream  networkStream)
		{
			byte [] headerDescriptor = new  byte[kHeaderDescriptorSize];
			byte [] headerDTOSize = new  byte[sizeof(uint)];
			
			networkStream.Read(headerDescriptor,0,kHeaderDescriptorSize);
					
			networkStream.Read(headerDTOSize,0,sizeof(uint));
			
			uint DTOSize = BitConverter.ToUInt32(headerDTOSize,0);
			
			System.UInt16 DTOHeader = BitConverter.ToUInt16(headerDescriptor,0);
			//Console.WriteLine("MUMIN: " + headerDTOSize.Length + " ok:" + sizeof(uint) + " -- " + headerDTOSize[0] + " " + headerDTOSize[1] + " " + headerDTOSize[2] + " " + headerDTOSize[3]);
			byte [] body = new byte[DTOSize];
			networkStream.Read(body, 0, (int)DTOSize);
			
			return converter.GetDTO((int)DTOHeader, body);
			
		}
		
		private void writeDTO (NetworkStream  networkStream, IDTO responseDTO)
		{
				byte [] header = new byte[kHeaderSize];
				
		
				byte [] headerDTOSize = BitConverter.GetBytes(converter.GetHeader(responseDTO));
				for (int i = 0; i < kHeaderDescriptorSize; i++)
					header[i] = headerDTOSize[i];

				MemoryStream DTOWriteStream = converter.GetDTOStream(responseDTO);
				
				int DTOSize = (int)DTOWriteStream.Length;
				for (int i = 0; i < sizeof(int); i++)
					header[i + kHeaderDescriptorSize] = BitConverter.GetBytes(DTOSize)[i];
				networkStream.Write(header,0,kHeaderSize);
				networkStream.Write(DTOWriteStream.GetBuffer(),0,(int)DTOSize);
				
				networkStream.Flush();			
		}
		
		public void Respond (object response)
		{			
			responseDTO = (IDTO) response;
			isWaitingForReply = false;
		}
		
		public void Stop()
		{
			networkStream.Close();
			client.Close();
			//client.Disconnect();
			IsTransfering = false;
		}
		
		public override int GetHashCode ()
		{
			return this.client.GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			Console.WriteLine("MAMA");
			if (obj is ISocketConnection)
			{
				return this.Equals((ISocketConnection)obj);
			}
			else
				return base.Equals (obj);
			
		}
		
		public bool Equals (ISocketConnection obj)
		{
			
			Console.WriteLine("HEJAPAA");
			
			if (obj == null)
				return false;
			
			return 	obj.Address == this.Address &&
				  	obj.Port == this.Port;
			/*
			if (obj is DTOConnection)
			{
				
				return ((DTOConnection)obj).clientEndpoint.Address == this.clientEndpoint.Address &&
					   ((DTOConnection)obj).clientEndpoint.Port == this.clientEndpoint.Port;
			}
			else
				return base.Equals (obj);
			*/
		}

	}
}
