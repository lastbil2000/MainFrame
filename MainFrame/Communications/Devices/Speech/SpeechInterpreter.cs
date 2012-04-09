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

		private Random _randomISH;
		
		private INervousSystem _mediator;
		private bool _isReady;
		private string _xmlLanguageFile;
		private List<string> _paragraphs;
		private System.Collections.Hashtable _paragraphsU;
		
		public SpeechInterpreter (string xmlLanguageFile)
		{
			_randomISH = new Random ( );
			this._xmlLanguageFile = xmlLanguageFile;
		}

		
		public string GetReply(string text)
		{
			
			foreach (string paragraph in _paragraphs)
			{
				if (new Regex("^" + paragraph).IsMatch(text.ToLower()))
				{
					
					foreach (string ss in (List<string>)_paragraphsU[paragraph])
						Console.WriteLine( " -- " + ss);
					int i = _randomISH.Next(((List<string>)_paragraphsU[paragraph]).Count);
					Console.WriteLine("regexp:{0} matches:{1}", paragraph.ToLower(), text.ToLower());
					string reply = ((List<string>)_paragraphsU[paragraph])[i];
					
					return reply;
				}
			}
			return null;
	
		}
		#region IRequestable implementation
		public T Request<T> (INervousSignal<T> message)
		{
			CommandInterpreter.CommandNotInterpretedSignal signal = message as CommandInterpreter.CommandNotInterpretedSignal;
				
			if (signal != null)
			{
				TryInterpret(signal.Data);
			}
			return default (T);
		}
		
		public void TryInterpret (string text) 
		{
			if (_isReady) 
			{
				_isReady = false;
				
					string reply = GetReply(text);
					
					if (reply != null)
						_mediator.Request(new EspeakTTS.SpeechSignal() { Data = reply });
				
				_isReady = true;
			}			
		}
		
		#endregion
		#region INode implementation
		public void SetMediator (INervousSystem mediator)
		{
			this._mediator = mediator;
		}
		
		
		public IEnumerable<Type> Subjects {
			get {
				if (_isReady)
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
			XPathDocument source = new XPathDocument (_xmlLanguageFile);
			XPathNodeIterator paragraphIterator = source.CreateNavigator().Select("//speech/p");
			
			_paragraphsU = new System.Collections.Hashtable();
			//paragraphs = new Dictionary<string, List<string>>();
			_paragraphs = new List<string>();
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
					_paragraphsU.Add(questionIterator.Current.Value.ToLower(), answers);
					//Console.WriteLine(questionIterator.Current.Value);
				}
				
			}
			
			foreach (string key in from s in _paragraphsU.Keys.Cast<string>() orderby s.Length descending select s)
			{
				
				//paragraphs.Add(key.ToLower(),(List<string>)paragraphsU[key]);
				_paragraphs.Add(key);
			}
			/*
			foreach (string key in paragraphs)
			{
				Console.WriteLine (key);
				foreach (string ss in (List<string>)paragraphsU[key])
					Console.WriteLine (" -- " + ss);
			}
			*/
			_isReady = true;
		}
		
		
		public void Stop ()
		{
			_isReady = false;
		}
		
		
		public bool Ready {
			get {
				return _isReady;
			}
		}
		
		#endregion
	}
}
