
using System;

namespace MainFrame.Communication.Log
{

	public enum LogTypes {
		Message,
		Warning,
		Error
	}

	public interface IMessageLogger
	{
		
		
		void Write(string message);
		void WriteLine(string message);
		
		void Write(string message, LogTypes logType);
		void WriteLine(string message, LogTypes logType);
		
		void AddLogger(IMessageLogger logger);

		
	}
}
