using System.Collections.Generic;
using System;

namespace MainFrame.Communication.Log
{


	public class StaticLogger : IMessageLogger
	{
		private List<IMessageLogger> loggers;
		protected static StaticLogger instance;
		
		public static IMessageLogger Instance 
		{
			get 
				
				//TODO: thread safety?
			{
				if (instance == null)
					instance = new StaticLogger();
				
				return instance;
			}
		}
		
		private StaticLogger ()
		{
			loggers = new List<IMessageLogger>();
		}
		
		public void AddLogger(IMessageLogger logger) 
		{
			loggers.Add(logger);
		}
		
		
		public void Write (string message)
		{
			foreach (IMessageLogger logger in loggers)
				logger.Write(message);
		}
		
		
		public void WriteLine (string message)
		{
			foreach (IMessageLogger logger in loggers)
				logger.WriteLine(message);
			
		}

		public void Write (string message, LogTypes logType)
		{
			foreach (IMessageLogger logger in loggers)
				logger.Write(message, logType);
		}
		
		
		public void WriteLine (string message, LogTypes logType)
		{
			foreach (IMessageLogger logger in loggers)
				logger.WriteLine(message, logType);
			
		}
		
		public static void d(string message) 
		{
			StaticLogger.Instance.WriteLine(message, LogTypes.Message);	
		}

		public static void w(string message) 
		{
			StaticLogger.Instance.WriteLine(message, LogTypes.Warning);	
		}

		public static void e(string message) 
		{
			StaticLogger.Instance.WriteLine(message, LogTypes.Error);	
		}
		
		public static void x(Exception ex) 
		{
			StaticLogger.Instance.WriteLine(ex.Message, LogTypes.Error);
			StaticLogger.Instance.WriteLine(ex.StackTrace, LogTypes.Error);
			StaticLogger.Instance.WriteLine(ex.Source);
		}

		
		~StaticLogger() 
		{
			instance = null;
		}

		
	}
}
