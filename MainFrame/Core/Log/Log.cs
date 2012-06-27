using System.Collections.Generic;
using System;
using System.Threading;

namespace MainFrame.Core.Logger
{


	public class Log : IMessageLogger
	{
		private List<IMessageLogger> loggers;
		protected static Log instance;
		protected bool _showThreadId;
		
		public static IMessageLogger Instance 
		{
			get 
				
				//TODO: thread safety?
			{
				if (instance == null)
					instance = new Log();
				
				return instance;
			}
		}
		
		private Log ()
		{
			loggers = new List<IMessageLogger>();
		}
		
		public void AddLogger(IMessageLogger logger) 
		{
			loggers.Add(logger);
		}
		
		
		public void Write (string message, MainFrame.Core.Logger.LogTypes logType, string tag)
		{
			foreach (IMessageLogger logger in loggers)
				logger.WriteLine(message, logType, tag);
		}
	
		public void WriteLine (string message, MainFrame.Core.Logger.LogTypes logType, string tag)
		{
			if (_showThreadId)
				message += "[" + Thread.CurrentThread.ManagedThreadId + "] ";
			
			foreach (IMessageLogger logger in loggers)
				logger.WriteLine(message, logType, tag);
		}
		
		public static void d(string message) 
		{
			Log.d (message, null);
		}

		public static void w(string message) 
		{
			Log.w (message, null);
		}

		public static void e(string message) 
		{
			Log.e (message, null);
		}

		public static void d(string message, object tag) 
		{
			Log.Instance.WriteLine(message, LogTypes.Message, tag != null ? tag.ToString() : null);	
		}

		public static void w(string message, object tag)  
		{
			Log.Instance.WriteLine(message, LogTypes.Warning, tag != null ? tag.ToString() : null);	
		}

		public static void e(string message, object tag) 
		{
			Log.Instance.WriteLine(message, LogTypes.Error, tag != null ? tag.ToString() : null);
		}
		
		public static void x(Exception ex) 
		{
			Log.Instance.WriteLine(ex.Message, LogTypes.Error, null);
			Log.Instance.WriteLine(ex.StackTrace, LogTypes.Error, null);
			Log.Instance.WriteLine(ex.Source, LogTypes.Error, null);
		}
		
		public static bool ShowThreadId 
		{
			set {

				Log.instance._showThreadId = value;
			}
		}
		
		~Log() 
		{
			instance = null;
		}

	}
}
