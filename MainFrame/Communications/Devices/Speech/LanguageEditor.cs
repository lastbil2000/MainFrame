using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using System;

namespace MainFrame.Devices.Speech
{


	public class LanguageEditor
	{
		private string xmlLanguageFile;
		private XmlDocument doc;
		
		public LanguageEditor (string xmlLanguageFile)
		{
			this.xmlLanguageFile  = xmlLanguageFile;
			XmlTextReader reader = new XmlTextReader(xmlLanguageFile);
			doc = new XmlDocument();
			doc.Load(reader);
			reader.Close();			
		}
		
		public void Insert(List<string> questions, List<string> answers) 
		{
			bool foundQuestion = false;
			
			foreach (string question in questions) 
			{
				XPathNodeIterator paragraphIterator = doc.CreateNavigator().Select("//speech/p");
				
				while (paragraphIterator.MoveNext())
				{
					if (HasQuestion(paragraphIterator.Current, question)) 
					{
						
						Console.WriteLine("question alrready found: " + question);
						//paragraphIterator.Current.AppendChild(
						foundQuestion = true;
					}
				}
			}
			
			if (!foundQuestion) {
				XmlElement newP = doc.CreateElement("p");
				foreach (string question in questions) 
				{
					XmlElement newQ = doc.CreateElement("q");
					newQ.InnerText = question.ToLower();
					newP.AppendChild(newQ);
				}
				
				foreach (string answer in answers) 
				{
					XmlElement newA = doc.CreateElement("a");
					newA.InnerText = answer.ToLower();
					newP.AppendChild(newA);
				}
				
				doc.CreateNavigator().SelectSingleNode("//speech").AppendChild("<p>" + newP.InnerXml + "</p>");
				//XmlTextWriter writer = new XmlTextWriter();
				doc.Save (xmlLanguageFile);
				//doc.Save(writer);
				//writer.Close();

			}
			
		}
		
		public void CompileLanguageFiles(string scriptDir, string baseDir, string targetDir) 
		{
			string command = "sh " + scriptDir + "/sphinx/create_language_files.sh " + baseDir + " language " + targetDir;
			
			Mono.Unix.Native.Stdlib.system(command);
		}
		
		private bool HasQuestion(XPathNavigator questionNode, string question) 
		{
			question = question.ToLower();
			
			XPathNodeIterator questionIterator = (XPathNodeIterator) questionNode.Select("q");
			while (questionIterator.MoveNext())
				if (((string)questionIterator.Current.Value).ToLower() == question)
					return true;
			
			return false;
		}
	}
}
