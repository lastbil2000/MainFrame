
using System;

namespace MainFrame.Communication.Log
{


	public class ConsoleLogger : IMessageLogger
	{

		public void Write (string message)
		{
			this.Write(message, LogTypes.Message);
		}
		
		public void WriteLine (string message)
		{
			this.WriteLine(message, LogTypes.Message);
		}
		
		public void Write(string message, LogTypes logType) 
		{
			ConsoleColor currentColor = Console.ForegroundColor;
			setConsoleColor(logType);
			
			Console.Write(message);
			
			Console.ForegroundColor = currentColor;
		}
		
		public void WriteLine(string message, LogTypes logType) 
		{
			ConsoleColor currentColor = Console.ForegroundColor;
			
			setConsoleColor(logType);
			Console.WriteLine(message);
			
			Console.ForegroundColor = currentColor;		
		}
		
		private void setConsoleColor (LogTypes logType) 
		{
			if (logType == LogTypes.Error)
				Console.ForegroundColor = ConsoleColor.Red;
			else if (logType == LogTypes.Warning)
				Console.ForegroundColor = ConsoleColor.Yellow;			
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
