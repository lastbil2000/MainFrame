using System;

namespace MainFrame.Speech
{


	public class TextReceivedEventArgs : EventArgs
	{
		public string Message {get; set;}
	}
	
	public class TextInterpretedEventArgs : EventArgs
	{
		public string Message {get; set;}
	}
	
	public class CommandReceivedEventArgs : EventArgs
	{
		public string Command {get; set;}
	}

	public class ErrorReceivedEventArgs : EventArgs
	{
		public string Message {get; set;}
		public int ErrorCode {get; set;}
	}
	
}
