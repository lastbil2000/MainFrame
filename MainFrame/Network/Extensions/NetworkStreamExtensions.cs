using System;
using System.Net.Sockets;
using System.Collections.Generic;

namespace MainFrame.Core.Network.Extensions
{
	public static class NetworkStreamExtensions
	{

		public static byte [] ReadBytes (this NetworkStream stream, int bytesExpected = -1) 
		{
			byte [] tmpBuffer = new byte [bytesExpected];
			
			int size = 0;
			int bytesRead = 0;
			IList<byte[]> chunks = new List<byte[]>();
			
			while (size < bytesExpected)
			{
				bytesRead = stream.Read(tmpBuffer, 0, tmpBuffer.Length);
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
		
	}
}

