using System.Xml.XPath;
using System.Xml;
using System.Xml.Xsl;
using System;
using System.Collections.Generic;

using ISE;
using MBF.Core;
using MBF.Speech;

using System.IO;

namespace MainFrame.Centra
{

	public class Brocas
	{

		private Random randomISH;
		
		protected System.Collections.Hashtable paragraphs;
		
		protected MBF.Speech.Flite tts;
		protected Elements.ASR _asr; //temptemp
		
		public Brocas (string xmlLanguageFile, Elements.ASR asr)
		{
			_asr = asr;
			randomISH = new Random ( );
			
			//TODO: handle double question entries
			XPathDocument source = new XPathDocument (xmlLanguageFile);
			XPathNodeIterator paragraphIterator = source.CreateNavigator().Select("//speech/p");
			
			paragraphs = new System.Collections.Hashtable();
			
			while (paragraphIterator.MoveNext())
			{
				List<string> answers = new List<string>();
				
				XPathNodeIterator answerIterator = (XPathNodeIterator) paragraphIterator.Current.Select("a");
				while (answerIterator.MoveNext())
					answers.Add(answerIterator.Current.Value);
				
				XPathNodeIterator questionIterator = (XPathNodeIterator) paragraphIterator.Current.Select("q");
				while (questionIterator.MoveNext())
				{
					paragraphs.Add(questionIterator.Current.Value.ToUpper(), answers);
					//Console.WriteLine(questionIterator.Current.Value);
				}
				
			}
			
			tts = new Flite();
			
		}
		
		public void TextReceived(object sender, Elements.TextReceivedEventArgs e)
		{
			//TODO: stop listening while talking! stupid computer.
			System.Console.WriteLine("------------");
			_asr.IsActive = false;
			if (paragraphs.ContainsKey(e.Message))
			{
				int i = randomISH.Next(((List<string>)paragraphs[e.Message]).Count -1);
				
				System.Console.WriteLine(i);
				string reply = ((List<string>)paragraphs[e.Message])[ 
				     i
				];
				System.Console.WriteLine("Replying with: " + reply);
				tts.SyntText (reply);
				
			}
			
			else
				tts.SyntText("I do not understand.");
			
			using ( FileStream stream = new FileStream( @"/tmp/flite.wav", FileMode.Create ) )
			using ( TextWriter writer = new StreamWriter( stream ) )
			{
			    writer.WriteLine( "" );
			}
			System.Console.WriteLine("------------------");
			_asr.IsActive = true;

		}
	}
}
