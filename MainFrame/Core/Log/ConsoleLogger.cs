
using System;

namespace MainFrame.Core.Logger
{


	public class ConsoleLogger : IMessageLogger
	{

		public void Write(string message, LogTypes logType, string tag) 
		{
			//ConsoleColor currentColor = Console.ForegroundColor;
			setConsoleColor(logType);
			
			if (tag == null)
				Console.Write(message);
			else
				Console.Write("[" + tag + "] " + message);
			
			//Console.ForegroundColor = currentColor;
		}
		
		public void WriteLine(string message, LogTypes logType, string tag) 
		{
			//ConsoleColor currentColor = Console.ForegroundColor;
			
			setConsoleColor(logType);
			if (tag == null)
				Console.WriteLine(message);
			else
				Console.WriteLine("[" + tag + "] " + message);
			
			//Console.ForegroundColor = currentColor;
		}
		
		private void setConsoleColor (LogTypes logType) 
		{
			if (logType == LogTypes.Error)
				Console.ForegroundColor = ConsoleColor.Red;
			else if (logType == LogTypes.Warning)
				Console.ForegroundColor = ConsoleColor.Yellow;
			else
				Console.ForegroundColor = ConsoleColor.Gray;
			
		}
		
		public void AddLogger(IMessageLogger logger) 
		{
			throw new NotImplementedException("CARAMBA!");
		}
		
		public ConsoleLogger ()
		{
		}
	}
}
