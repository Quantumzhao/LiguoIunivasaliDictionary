using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Net;
using System.Resources;

namespace LigouniDictionary
{
	class Program
	{
		public static List<Lexicon> dictionaryBuffer = new List<Lexicon>();
		private static bool devMode = false;
		private static Properties.Settings Settings = new Properties.Settings();

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
					"    [5]Open a local dictionary\n" +
					"    [6]Edit Settings\n" + 
					"    [X]Exit\n");

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
						openADictionary();
						break;

					case ConsoleKey.D6:
						editSettings();
						break;

					case ConsoleKey.X:
					case ConsoleKey.Delete:
					case ConsoleKey.Backspace:
					case ConsoleKey.Escape:
						Environment.Exit(0);
						break;

					case ConsoleKey.D:
						devMode = true;
						goto default;

					case ConsoleKey.M:
						if (devMode)
						{
							DevModeEntry();
						}
						else
						{
							goto default;
						}
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
			Console.WriteLine("Loading Resource...");

			using (WebClient client = new WebClient())
			{
				using (Stream stream = client.OpenRead(
					@"https://terpconnect.umd.edu/~yishanzh/Dictionary.xml"))
				{
					XmlHelper xmlHelper = new XmlHelper(stream);
					dictionaryBuffer = xmlHelper.Parse();
				}
			}

			Console.WriteLine("Complete");
		}

		static void FindEngDef()
		{
			Console.WriteLine("Please Enter Your Word in Ligouni");

			string input = Console.ReadLine();
			Console.WriteLine();

			var list = from item in dictionaryBuffer
					   where item.Vocab.CompleteWord.Equals(input)
					   select item;

			Lexicon word;

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

			IEnumerable<Lexicon> retrievedList =
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

				dictionaryBuffer.Add(new Lexicon()
				{
					Definition = definition,
					Vocab = new Word()
					{
						Prefix = prefix,
						Stem = morpheme,
						Suffix = suffix
					}
				});

				XmlHelper.WriteToXml(dictionaryBuffer);

				if (!Settings.IsKeepTempFile)
				{
					File.Delete("temp.xml");
				}
			}
			catch
			{
				
			}
		}

		static void Print(IEnumerable<Lexicon> List)
		{
			foreach (Lexicon item in List)
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

				List<string> morpheme = item.Vocab.Stem;
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

		private static void editSettings()
		{
			Console.WriteLine("[1]Keep Temporary File : {0}", Settings.IsKeepTempFile);
			Console.WriteLine("Please enter the modification in the following format:");
			Console.WriteLine("[No.],Value");
			string[] input = Console.ReadLine().Split(',');
			try
			{
				if (int.TryParse(input[0].Trim('[', ']'), out int res))
				{
					switch (res)
					{
						case 1:
							Settings.IsKeepTempFile = bool.Parse(input[1]);
							break;

						default:
							break;
					}
				}
			}
			catch (Exception)
			{

				throw;
			}
		}

		static void Sort()
		{
			throw new NotImplementedException();
		}

		static void DevModeEntry()
		{
			Console.WriteLine("Developer Mode Activated");

			Console.WriteLine("Please enter the SftpPlugin Directory");
			string address = Console.ReadLine();
			Settings.SftpClientAddress = address;
		}

		private static void openADictionary()
		{
			Console.WriteLine("Please enter the path of your local dictionary");
			string address = Console.ReadLine();

			using (StreamReader reader = new StreamReader(address))
			{
				XmlHelper helper = new XmlHelper(reader.BaseStream);
				helper.Parse();
			}
		}
	}

	class Lexicon
	{
		public Word Vocab { get; set; }

		public string Definition { get; set; }
	}

	class Word
	{
		private List<string> stem = new List<string>();
		public List<string> Stem
		{
			get => stem;

			set
			{
				stem = value;

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

			foreach (var item in stem)
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
		public XmlDocument data { get; set; } = new XmlDocument();

		public XmlHelper(Stream stream)
		{
			data.Load(stream);
		}

		public List<Lexicon> Parse()
		{
			XmlNode dictionary = data["dictionary"];

			List<Lexicon> vocabularies = new List<Lexicon>();

			foreach (XmlNode xmlVocabulary in dictionary)
			{
				XmlNode xmlWord = xmlVocabulary.SelectSingleNode("word");
				Word word = new Word();

				word.Prefix = xmlWord["prefix"].InnerText.Split(',').ToList();
				word.Stem = xmlWord["morpheme"].InnerText.Split(',').ToList();
				word.Suffix = xmlWord["suffix"].InnerText.Split(',').ToList();

				string definition = xmlVocabulary.SelectSingleNode("definition").InnerText;

				vocabularies.Add(new Lexicon() { Vocab = word, Definition = definition});
			}

			return vocabularies;
		}

		public static void WriteToXml(List<Lexicon> dictionary, string uri = "")
		{
			using (FileStream fs = File.Create("temp.xml"))
			{
				using (XmlWriter writer = XmlWriter.Create(fs))
				{
					writer.WriteStartDocument();
					writer.WriteStartElement("dictionary");
					foreach (Lexicon lexicon in dictionary)
					{
						AddNode(
							writer, 
							"vocabulary", 
							() => {
								AddNode(
									writer,
									"word",
									() => {
										AddNode(
											writer,
											"prefix",
											new Func<string>(() => {
												return Concatenate(lexicon.Vocab.Prefix);
											})()
										);
										AddNode(
											writer,
											"morpheme",
											new Func<string>(() => {
												return Concatenate(lexicon.Vocab.Stem);
											})()
										);
										AddNode(
											writer,
											"suffix",
											new Func<string>(() => {
												return Concatenate(lexicon.Vocab.Suffix);
											})()
										);
									}
								);
								AddNode(writer, "definition", lexicon.Definition);
							}
						);
					}
					writer.WriteEndElement();
					writer.WriteEndDocument();
				}
			}
		}

		private static void AddNode(XmlWriter writer, string name, Action addNode = null)
		{
			writer.WriteStartElement(name);
			addNode?.Invoke();
			writer.WriteEndElement();
		}
		private static void AddNode(XmlWriter writer, string name, string content)
		{
			writer.WriteStartElement(name);
			writer.WriteString(content);
			writer.WriteEndElement();
		}

		private static string Concatenate(List<string> list)
		{
			StringBuilder tempString = new StringBuilder();
			foreach (var item in list)
			{
				tempString.Append(item);
				tempString.Append(',');
			}
			tempString.Remove(tempString.Length - 1, 1);
			return tempString.ToString();
		}
	}
}
