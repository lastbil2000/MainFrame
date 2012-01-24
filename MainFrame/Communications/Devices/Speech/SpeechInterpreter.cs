using System.Xml.XPath;
using System.Xml;
using System.Xml.Xsl;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using MainFrame.Devices;
using MainFrame.Communication;

namespace MainFrame.Devices.Speech
{

	public class SpeechInterpreter  : IRequestable, IDevice
	{

		protected Random randomISH;
		
		protected INervousSystem mediator;
		protected bool isReady;
		protected string xmlLanguageFile;
		protected List<string> paragraphs;
		System.Collections.Hashtable paragraphsU;
		
		public SpeechInterpreter (string xmlLanguageFile)
		{
			randomISH = new Random ( );
			this.xmlLanguageFile = xmlLanguageFile;
		}

		
		public string GetReply(string text)
		{
			
			foreach (string paragraph in paragraphs)
			{
				if (new Regex("^" + paragraph).IsMatch(text.ToLower()))
				{
					
					foreach (string ss in (List<string>)paragraphsU[paragraph])
						Console.WriteLine( " -- " + ss);
					int i = randomISH.Next(((List<string>)paragraphsU[paragraph]).Count);
					Console.WriteLine("regexp:{0} matches:{1}", paragraph.ToLower(), text.ToLower());
					string reply = ((List<string>)paragraphsU[paragraph])[i];
					
					return reply;
				}
			}
			return null;
	
		}
		#region IRequestable implementation
		public T Request<T> (INervousSignal<T> message)
		
		{
			if (isReady) 
			{
				isReady = false;
				CommandInterpreter.CommandNotInterpretedSignal signal = message as CommandInterpreter.CommandNotInterpretedSignal;
				
				if (signal != null)
				{
					string reply = GetReply(signal.Data);
					
					if (reply != null)
						mediator.Request(new EspeakTTS.SpeechSignal() { Data = reply });
				}
				isReady = true;
			}
			return default (T);
		}
		
		#endregion
		#region INode implementation
		public void SetMediator (INervousSystem mediator)
		{
			this.mediator = mediator;
		}
		
		
		public IEnumerable<Type> Subjects {
			get {
				if (isReady)
					yield return typeof (CommandInterpreter.CommandNotInterpretedSignal);
				else
					yield break;
			}
		}
		
		#endregion
		#region IDevice implementation
		public void Start ()
		{
						//TODO: handle double question entries
			XPathDocument source = new XPathDocument (xmlLanguageFile);
			XPathNodeIterator paragraphIterator = source.CreateNavigator().Select("//speech/p");
			
			paragraphsU = new System.Collections.Hashtable();
			//paragraphs = new Dictionary<string, List<string>>();
			paragraphs = new List<string>();
			//List<string> unsortedP = new List<string>();

			while (paragraphIterator.MoveNext())
			{
				
				List<string> answers = new List<string>();
				
				XPathNodeIterator answerIterator = (XPathNodeIterator) paragraphIterator.Current.Select("a");
				while (answerIterator.MoveNext())
					answers.Add(answerIterator.Current.Value);
				
				XPathNodeIterator questionIterator = (XPathNodeIterator) paragraphIterator.Current.Select("q");
				while (questionIterator.MoveNext())
				{
					paragraphsU.Add(questionIterator.Current.Value.ToLower(), answers);
					//Console.WriteLine(questionIterator.Current.Value);
				}
				
			}
			
			foreach (string key in from s in paragraphsU.Keys.Cast<string>() orderby s.Length descending select s)
			{
				
				//paragraphs.Add(key.ToLower(),(List<string>)paragraphsU[key]);
				paragraphs.Add(key);
			}
			/*
			foreach (string key in paragraphs)
			{
				Console.WriteLine (key);
				foreach (string ss in (List<string>)paragraphsU[key])
					Console.WriteLine (" -- " + ss);
			}
			*/
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
	}
}
