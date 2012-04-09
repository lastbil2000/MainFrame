using System.Collections.Generic;
using System;
using System.Xml.XPath;
using System.Xml;
using System.Xml.Xsl;
using System.Linq;
using MainFrame.Devices;
using MainFrame.Communication;

namespace MainFrame.Devices.Speech
{

	public class CommandInterpreter : IDevice, IRequestable
	{

		
		protected List<string> commands;
		protected INervousSystem mediator;
		protected bool isReady;
		
		public class CommandNotInterpretedSignal : INervousSignal<string>
		{
			public string Identifier {get; set;}
			public string Data{get; set;}
			
			public CommandNotInterpretedSignal() 
			{
				Type = typeof(CommandInterpreter.CommandNotInterpretedSignal);
			}
			public Type Type{get; set;}
		}
		
		public CommandInterpreter ( string xmlLanguageFile)
		{
			XPathDocument source = new XPathDocument (xmlLanguageFile);
			XPathNodeIterator commandIterator = source.CreateNavigator().Select("//commands/command");
			
			commands = new List<string>();
			
			while (commandIterator.MoveNext())
			{
				commands.Add(commandIterator.Current.Value.ToLower());
			}
			
		}
		
		public bool CanInterpret (string text)
		{
			/*
			TextReceivedEventArgs eArgs = (TextReceivedEventArgs)e;
			*/
			
			if ((from c in commands where text.ToLower().Contains(c) select c).Any())
				return true;
			else
				return false;			
			
		}
		
		#region IDevice implementation
		public void Start ()
		{
			isReady = true;
		}
		
		
		public void Stop ()
		{
			isReady = false;
		}
		
		
		public bool Ready {
			get {
				return isReady;
			}
		}
		
		#endregion
		
		
		#region IRequestable implementation
		public T Request<T> (INervousSignal<T> message)		
		{
			if (isReady) 
			{
				SphinxASRProcess.TextReceived textReceived = message as SphinxASRProcess.TextReceived;
				
				if (textReceived != null )
				{
					if (CanInterpret(textReceived.Data))
						ExecuteCommand(textReceived.Data.ToLower());
					else
						mediator.Request(new CommandNotInterpretedSignal() {Data = textReceived.Data});
				}
			}
			return default (T);
		}
		#endregion
		
		//TODO: script stuff like this, but how?
		private void ExecuteCommand (string command) 
		{
			Arm3Engines engine = mediator.Request<Arm3Engines>(new GetSignal<Arm3Engines>());

			if (engine == default(Arm3Engines))
			{
				Console.WriteLine ("No Arm3Engine identified");
				return;
			}
			if (command.Contains("open")) 
				engine.Open = true;
			else if (command.Contains("close")) 
				engine.Open = false;
			else if (command.Contains("arm"))
			{
				Console.WriteLine("this is the command: " + command);
				int speed = 5;
				if (command.Contains("ten"))
					speed = 20;
				if (command.Contains("up") || command.Contains("off") )
				    engine.Up -= speed;
				else if (command.Contains("down"))
				    engine.Up += speed;
				else if (command.Contains("left") || command.Contains("let"))
				    engine.Rotation -= speed;
				else if (command.Contains("right"))
				    engine.Rotation += speed;
			}			
		}
		
		#region INode implementation
		public void SetMediator (INervousSystem mediator)
		{
			this.mediator = mediator;
		}
		
		
		public IEnumerable<Type> Subjects {
			get {
				if (isReady)
					yield return typeof(SphinxASRProcess.TextReceived);
				else
					yield break;
			}
		}
		
		#endregion
		
	}
}
