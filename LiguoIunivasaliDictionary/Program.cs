using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Net;

namespace LigouniDictionary
{
	class Program
	{
		public static List<Vocabulary> dictionaryBuffer = new List<Vocabulary>();
		static XmlHelper XmlHelper { get; set; } = new XmlHelper(WebHelper.GetStream());

		static void Main(string[] args)
		{
			Initiate();			

			while (true)
			{
				Console.WriteLine("Please Choose an Action: \n" +
					"    [1]Find the English Definition of a Word in Ligouni\n" +
					"    [2]Find the Word in Ligouni\n" +
					"    [3]Add a new vocabulary to Dictionary\n" +
					"    [4]Print the dictionary on Screen\n" + 
					"    [5]Exit\n");

				ConsoleKey key = Console.ReadKey().Key;

				Console.WriteLine("\n\n");

				switch (key)
				{
					case ConsoleKey.D1:
						FindEngDef();
						break;

					case ConsoleKey.D2:
						FindLigoUniWord();
						break;

					case ConsoleKey.D3:
						EditVocab();
						break;

					case ConsoleKey.D4:
						Print(dictionaryBuffer);
						break;

					case ConsoleKey.D5:
					case ConsoleKey.Delete:
					case ConsoleKey.Backspace:
					case ConsoleKey.Escape:
						Environment.Exit(0);
						break;

					default:
						Console.WriteLine("Invalid Input\n");
						break;
				}
			}
		}

		static void Initiate()
		{
			Console.ForegroundColor = ConsoleColor.Green;

			dictionaryBuffer = XmlHelper.Parse();
		}

		static void FindEngDef()
		{
			Console.WriteLine("Please Enter Your Word in Ligouni");

			string input = Console.ReadLine();
			Console.WriteLine();

			var list = from item in dictionaryBuffer
					   where item.Vocab.CompleteWord.Equals(input)
					   select item;

			Vocabulary word;

			try
			{
				word = list.Single();

				Console.WriteLine($"{word.Definition}\n");
			}
			catch
			{
				Console.WriteLine("\nNo match Word\n");
			}
		}

		static void FindLigoUniWord()
		{
			Console.WriteLine("Please Enter Your English Word. \nWe are going to find the matched ligouni word");

			string input = Console.ReadLine();

			IEnumerable<Vocabulary> retrievedList =
				from vocabulary in dictionaryBuffer
				where vocabulary.Definition.Contains(input)
				select vocabulary;

			if (retrievedList.ToList().Count == 0)
			{
				Console.WriteLine("Sorry, we found nothing\n");

				return;
			}

			Print(retrievedList);
		}

		static void EditVocab()
		{
			Console.WriteLine("Function not implemented yet! \n");
			Console.WriteLine("Are you sure to continue? \nYour input will make no effect. ");
			Console.WriteLine("[Y/N]");

			string key = Console.ReadKey().KeyChar.ToString().ToUpper();

			if (!key.Equals("Y"))
			{
				return;
			}

			Console.WriteLine("\nPlease enter your new vocabulary in the following format: ");
			Console.WriteLine("SPACE MATTERS! Please strictly follow the format!");
			Console.WriteLine("[Prefix0] [Prefix1] [...],[Morpheme0] [Morpheme1] [...],[Suffix0] [Suffix1] [...];[Definition]");

			string input = Console.ReadLine();

			try
			{
				string[] semicolumnSeperated = input.Split(';');

				string definition = semicolumnSeperated[1];

				string[] commaSeperated = semicolumnSeperated[0].Split(',');

				List<string> prefix = commaSeperated[0].Split(' ').ToList();
				List<string> morpheme = commaSeperated[1].Split(' ').ToList();
				List<string> suffix = commaSeperated[2].Split(' ').ToList();

				dictionaryBuffer.Add(new Vocabulary()
				{
					Definition = definition,
					Vocab = new Word()
					{
						Prefix = prefix,
						Morpheme = morpheme,
						Suffix = suffix
					}
				});
			}
			catch (Exception)
			{

				throw;
			}
		}

		static void Print(IEnumerable<Vocabulary> List)
		{
			foreach (Vocabulary item in List)
			{
				List<string> prefix = item.Vocab.Prefix;
				for (int i = 0; i < prefix.Count; i++)
				{
					if (i % 2 == 0)
					{
						Console.ForegroundColor = ConsoleColor.Blue;
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.DarkBlue;
					}

					Console.Write(prefix[i]);
				}

				List<string> morpheme = item.Vocab.Morpheme;
				for (int i = 0; i < morpheme.Count; i++)
				{
					if (i % 2 == 0)
					{
						Console.ForegroundColor = ConsoleColor.Gray;
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.DarkGray;
					}

					Console.Write(morpheme[i]);
				}

				List<string> suffix = item.Vocab.Suffix;
				for (int i = 0; i < suffix.Count; i++)
				{
					if (i % 2 == 0)
					{
						Console.ForegroundColor = ConsoleColor.Cyan;
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.DarkCyan;
					}

					Console.Write(suffix[i]);
				}

				Console.CursorLeft = 20;
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine(item.Definition);
			}

			Console.WriteLine();
		}

		static void Sort()
		{
			throw new NotImplementedException();
		}
	}

	class Vocabulary
	{
		public Word Vocab { get; set; }

		public string Definition { get; set; }
	}

	class Word
	{
		private List<string> morpheme = new List<string>();
		public List<string> Morpheme
		{
			get => morpheme;

			set
			{
				morpheme = value;

				updateCompleteWord();
			}
		}

		private List<string> prefix = new List<string>();
		public List<string> Prefix
		{
			get => prefix;

			set
			{
				prefix = value;

				updateCompleteWord();
			}
		}

		List<string> suffix = new List<string>();
		public List<string> Suffix
		{
			get => suffix;

			set
			{
				suffix = value;

				updateCompleteWord();
			}
		}
		
		public string CompleteWord { get; private set; }

		private void updateCompleteWord()
		{
			StringBuilder stringBuilder = new StringBuilder();

			foreach (var item in Prefix)
			{
				stringBuilder.Append(new StringBuilder(item));
			}

			foreach (var item in morpheme)
			{
				stringBuilder.Append(new StringBuilder(item));
			}

			foreach (var item in Suffix)
			{
				stringBuilder.Append(new StringBuilder(item));
			}

			CompleteWord = stringBuilder.ToString();
		}
	}

	class XmlHelper
	{
		private string localAddress;

		public XmlDocument data { get; set; } = new XmlDocument();

		public XmlHelper(string address)
		{
			ReadToMemory(address);
			localAddress = address;
		}
		public XmlHelper(Stream stream)
		{
			ReadToMemory(stream);
		}

		public void ReadToMemory(string address)
		{
			data.Load(address);
		}
		public void ReadToMemory(Stream stream)
		{
			data.Load(stream);
		}

		public List<Vocabulary> Parse()
		{
			XmlNode dictionary = data["dictionary"];

			List<Vocabulary> vocabularies = new List<Vocabulary>();

			foreach (XmlNode xmlVocabulary in dictionary)
			{
				XmlNode xmlWord = xmlVocabulary.SelectSingleNode("word");
				Word word = new Word();

				word.Prefix = xmlWord["prefix"].InnerText.Split(',').ToList();
				word.Morpheme = xmlWord["morpheme"].InnerText.Split(',').ToList();
				word.Suffix = xmlWord["suffix"].InnerText.Split(',').ToList();

				string definition = xmlVocabulary.SelectSingleNode("definition").InnerText;

				vocabularies.Add(new Vocabulary() { Vocab = word, Definition = definition});
			}

			return vocabularies;
		}

		public void WriteToXml(Stream stream)
		{
			//XmlWriter writer = XmlWriter.Create(stream);

			
		}

		public void AddNode(Vocabulary newVocabulary)
		{/*
			XmlNode dictionary = tempData["dictionary"];

			XmlElement newVocabulary = tempData.CreateElement("vocabulary");
			XmlElement newWord = tempData.CreateElement("word");
			XmlElement newDefinition = tempData.CreateElement("definition");
			XmlElement newPrefix = tempData.CreateElement("prefix");
			XmlElement newMorpheme = tempData.CreateElement("morpheme");
			XmlElement newSuffix = tempData.CreateElement("suffix");

			newPrefix.InnerText

				dictionary.AppendChild
				(

				);*/
		}
	}

	class WebHelper
	{
		public static WebClient WebClient { get; set; } = new WebClient();

		public static Stream GetStream()
		{
			return WebClient.OpenRead("https://quantumzhao.github.io/bulletins/Dictionary.xml");
		}
	}
}
