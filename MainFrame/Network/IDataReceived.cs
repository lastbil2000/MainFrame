using System;

namespace MainFrame.Core.Network
{
	internal interface IDataReceived<T>
	{
		void DataReceived(T data, string ip);
	}
}

