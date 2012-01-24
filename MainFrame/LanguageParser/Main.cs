using System;
using System.Xml.XPath;
using System.Xml;
using System.Xml.Xsl;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace LanguageParser
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			if (args.Length < 6 || args.Length % 2 != 0)
			{
				Console.WriteLine(@"Usage:\n ./LanguageParser -s <input file name> -lm <output model file name> -dict <output corpus> [-import <import dictionary>]");
				return;
			}
			
			string sourceFileName = null;
			string modelFileName = null;
			string corpusFileName = null;
			string importFileName = null;
			
			for (int i = 0; i < args.Length; i++)
			{
				if(args[i] == "-lm")
					modelFileName = args[i++ +1];
				else if(args[i] == "-dict")
					corpusFileName = args[i++ +1];
				else if(args[i] == "-s")
					sourceFileName = args[i++ +1];
				else if(args[i] == "-import")
					importFileName = args[i++ +1];
			}
			Console.WriteLine(@"Using: " + sourceFileName + " as source...\n");
			
			//reading data from source file:
			XPathDocument source = new XPathDocument (sourceFileName);
			
			List<string> output = new List<string>();
			List<string> words = new List<string>();
			//HashTable output = new HashTable();			
			//HashTable words = new HashTable();
			
			XPathNodeIterator inputIterator = source.CreateNavigator().Select("//q | //command");
			
			while (inputIterator.MoveNext() )
			{
				//remove everything that is not letters or spaces
				string line = new Regex(@"[^a-z\s\']+").Replace(inputIterator.Current.Value.ToLower().Trim(), "");
				//adding lines to comprhends (sentences or words);
				output.Add(line);
				foreach (string word in new Regex(@"[\s]+").Split(line))
					if (!words.Contains(word))
						words.Add(word);
			}
			
			List<string> originalWords = words;
			List<string> originalOutput = output;
			
			//read data from import dictionary file
			if (importFileName != null)
			{
				Console.WriteLine("Reading import file: " + importFileName);
				string [] lines = File.ReadAllLines(importFileName);
				int lineCount = 0;
				int percentage = 0;
				string previousWord = "";
				foreach (string line in lines)
				{
					string word = new Regex(@"[a-z\']+").Match(line.ToLower()).Value;
					if (word.Length > 0 && word != previousWord)
					{
						
						if (!originalWords.Contains(word))
							words.Add(word);
						if (!originalOutput.Contains(word))
							output.Add(word);
						
						previousWord = word;
					}
					if ((int)(((double)lineCount++/(double)lines.Length) * 100) > percentage)
					{
						Console.Write( " [ " + percentage++ + "% ]");
						Console.SetCursorPosition(0, Console.CursorTop);
					}
				}
				Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop);
				Console.Write ("\n");
				
				
				
			}
			
			if (File.Exists(modelFileName))
				File.Delete(modelFileName);
			if (File.Exists(corpusFileName))
				File.Delete(corpusFileName);
			
			using (StreamWriter modelWriter = File.AppendText(modelFileName))
			{
				foreach (string outStr in output)
					modelWriter.WriteLine("<s> " + outStr.ToUpper() + " </s>");
					//modelWriter.WriteLine( outStr);
	
				modelWriter.Close();
			}
			
			/*
			List<string> words = new List<string>();

			foreach (string outStr in output)
				foreach (string word in  new Regex(@"\s").Split(outStr))
					if (!words.Contains(word.Trim().ToUpper()))
							words.Add(word.Trim().ToUpper());
			
			*/
			words.Sort();
			
			using (StreamWriter corpuslWriter = File.AppendText(corpusFileName))
			{
				foreach (string word in words)
					corpuslWriter.WriteLine(word);
				

				corpuslWriter.Close();
			}
			
		}
	}
}
