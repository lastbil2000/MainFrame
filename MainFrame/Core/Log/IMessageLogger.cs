
using System;

namespace MainFrame.Core.Logger
{

	public enum LogTypes {
		Message,
		Warning,
		Error
	}

	public interface IMessageLogger
	{
		

		void Write(string message, LogTypes logType, string tag);
		void WriteLine(string message, LogTypes logType, string tag);

		
		void AddLogger(IMessageLogger logger);

		
	}
}
