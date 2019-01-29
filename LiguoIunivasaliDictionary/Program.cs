using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Net;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LigouniDictionary
{
	class Program
	{
		public static List<Lexicon> dictionaryBuffer = new List<Lexicon>();
		private static bool devMode = false;

		static void Main(string[] args)
		{
			Initiate();

			while (true)
			{
				Console.WriteLine("Please Choose an Action: \n" +
					"    [1] Find the English Definition of a Word in Ligouni\n" +
					"    [2] Find the Word in Ligouni\n" +
					"    [3] Edit lexicon\n" +
					"    [4] Print the dictionary on Screen\n" + 
					"    [5] Open a local dictionary\n" +
					"    [6] Edit Settings\n" + 
					"    [7] Sort the dictionary\n" +
					"    [X] Exit\n");

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

					case ConsoleKey.D7:
						Sort();
						break;

					case ConsoleKey.X:
					case ConsoleKey.Delete:
					case ConsoleKey.Backspace:
					case ConsoleKey.Escape:
						Environment.Exit(0);
						break;

					case ConsoleKey.D:
						devMode = !devMode;
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

					case ConsoleKey.V:
						if (devMode)
						{
							massiveModificationMode();
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

			try
			{
				updateDictionary();
				Console.WriteLine("Complete");
			}
			catch (Exception e)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				if (e is WebException)
					Console.WriteLine("Connection Failed");
				Console.WriteLine(e.Message);
				Console.ForegroundColor = ConsoleColor.Green;

				Console.WriteLine("Trying to retrive local cache");
				openADictionary();
			}
		}

		private static void updateDictionary()
		{
			try
			{
				using (WebClient client = new WebClient())
				{
					using (Stream stream = client.OpenRead(
						@"https://terpconnect.umd.edu/~yishanzh/Dictionary.xml"))
					{
						XmlHelper xmlHelper = new XmlHelper(stream);
						dictionaryBuffer = xmlHelper.Parse();
					}
				}
			}
			catch
			{
				throw;
			}
		}

		static void FindEngDef()
		{
			Console.WriteLine("Please Enter Your Word in Ligouni");

			string input = Console.ReadLine();
			Console.WriteLine();

			try
			{
				Console.WriteLine(
					$"{dictionaryBuffer.Where(l => l.Vocab.CompleteWord == input).Single().Definition}\n");
			}
			catch
			{
				Console.WriteLine("\nNo match Word\n");
			}
		}

		static void FindLigoUniWord()
		{
			Console.WriteLine(
				"Please Enter Your English Word. \nWe are going to find the matched ligouni word");

			string input = Console.ReadLine();

			List<Lexicon> retrievedList = 
				dictionaryBuffer.Where(l => l.Definition.Contains(input)).ToList();

			if (retrievedList.Count == 0)
			{
				Console.WriteLine("Sorry, we found nothing\n");

				return;
			}

			Print(retrievedList);
		}

		static void EditVocab()
		{
			Console.WriteLine("Do you wish to: \n" +
				"    [1]Add a vocabulary\n" +
				"    [2]Update a vocabulary\n" +
				"    [3]Delete a vocabulary\n");
			ConsoleKey key = Console.ReadKey().Key;
			Console.WriteLine();

			try
			{
				switch (key)
				{
					case ConsoleKey.D1:
						AddVocab();
						break;

					case ConsoleKey.D2:
						UpdateVocab();
						break;

					case ConsoleKey.D3:
						DeleteVocab();
						break;

					default:
						Console.WriteLine("Invalid Input");
						return;
				}

				XmlHelper.WriteToXml(dictionaryBuffer);

				if (devMode)
				{
					SftpWrite();
				}

				JsonHelper helper = new JsonHelper("config.json");
				if (!helper.GetProperty("IsKeepTempFile").Value<bool>())
				{
					File.Delete("temp.xml");
				}

				updateDictionary();
			}
			catch (Exception e)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(e.Message);
				Console.ForegroundColor = ConsoleColor.Green;
			}
		}
		private static void AddVocab()
		{
			Console.WriteLine("\nPlease enter your new vocabulary in the following format: ");
			Console.WriteLine("SPACE MATTERS! Please strictly follow the format!");
			Console.WriteLine("[Prefix0] [Prefix1] [...],[Morpheme0] [Morpheme1] [...],[Suffix0] [Suffix1] [...];[Definition]");

			string input = Console.ReadLine();

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
		}
		private static void DeleteVocab()
		{
			Console.WriteLine("Please enter the the word in ligouni that you wish to delete");
			string input = Console.ReadLine();

			for (int i = 0; i < dictionaryBuffer.Count; i++)
			{
				if (dictionaryBuffer[i].Vocab.CompleteWord == input)
				{
					dictionaryBuffer.RemoveAt(i);
					return;
				}
			}

			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Target not found");
			Console.ForegroundColor = ConsoleColor.Green;
		}
		private static void UpdateVocab()
		{
			Console.WriteLine("Please enter the vocabulary that you wish to update");
			string input = Console.ReadLine();

			Lexicon lexicon = null;
			for (int i = 0; i < dictionaryBuffer.Count; i++)
				if (dictionaryBuffer[i].Vocab.CompleteWord == input)
					lexicon = dictionaryBuffer[i];

			if (lexicon == null)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Target not found");
				Console.ForegroundColor = ConsoleColor.Green;
				return;
			}

			Console.WriteLine("\nPlease enter your new vocabulary in the following format: ");
			Console.WriteLine("SPACE MATTERS! Please strictly follow the format!");
			Console.WriteLine("[Prefix0] [Prefix1] [...],[Morpheme0] [Morpheme1] [...],[Suffix0] [Suffix1] [...];[Definition]");
			input = Console.ReadLine();
			string[] semicolumnSeperated = input.Split(';');
			string definition = semicolumnSeperated[1];
			string[] commaSeperated = semicolumnSeperated[0].Split(',');

			lexicon.Vocab.Prefix = commaSeperated[0].Split(' ').ToList();
			lexicon.Vocab.Stem = commaSeperated[1].Split(' ').ToList();
			lexicon.Vocab.Suffix = commaSeperated[2].Split(' ').ToList();
			lexicon.Definition = semicolumnSeperated[1];
		}

		static void Print(IEnumerable<Lexicon> List)
		{
			foreach (Lexicon item in List)
			{
				List<string> prefix = item.Vocab.Prefix;
				for (int i = 0; i < prefix.Count; i++)
				{
					if (i % 2 == 0) Console.ForegroundColor = ConsoleColor.Blue;
					else Console.ForegroundColor = ConsoleColor.DarkBlue;

					Console.Write(prefix[i]);
				}

				List<string> morpheme = item.Vocab.Stem;
				for (int i = 0; i < morpheme.Count; i++)
				{
					if (i % 2 == 0) Console.ForegroundColor = ConsoleColor.Gray;
					else Console.ForegroundColor = ConsoleColor.DarkGray;

					Console.Write(morpheme[i]);
				}

				List<string> suffix = item.Vocab.Suffix;
				for (int i = 0; i < suffix.Count; i++)
				{
					if (i % 2 == 0) Console.ForegroundColor = ConsoleColor.Cyan;
					else Console.ForegroundColor = ConsoleColor.DarkCyan;

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
			using (JsonHelper config = new JsonHelper("config.json"))
			{
				int count = 0;
				foreach (var item in config.ListProperties())
				{
					Console.WriteLine($"[{count}] {item.Name} {item.Value.ToString()}");
					count++;
				}

				Console.WriteLine("Please enter the new value");
				Console.ForegroundColor = ConsoleColor.Blue;
				Console.WriteLine("Format: [No.],[Value]");
				Console.ForegroundColor = ConsoleColor.Green;

				string[] input = Console.ReadLine().Split(',').Select(s => s.Trim('[', ']')).ToArray();
				if (int.TryParse(input[0], out int num))
					config.SetProperty<JProperty>(config.ListProperties().ToArray()[num].Name, input[1]);
			}
		}

		static void Sort()
		{
			dictionaryBuffer = dictionaryBuffer.OrderBy(l => l.Vocab.CompleteWord).ToList();
			XmlHelper.WriteToXml(dictionaryBuffer);
			if (devMode) SftpWrite();
		}

		static void DevModeEntry()
		{
			Console.WriteLine("Developer Mode Activated");

			Console.WriteLine("Please enter the SftpPlugin Directory");
			string address = Console.ReadLine();

			using (JsonHelper config = new JsonHelper("config.json"))
			{
				if (config.Has("SftpClientAddress"))
				{
					if (address == "") return;
					config.SetProperty<string>("SftpClientAddress", address);
				}
				else config.AddProperty("SftpClientAddress", address);
			}
		}

		private static void openADictionary()
		{
			Console.WriteLine("Please enter the path of your local dictionary");
			string address = Console.ReadLine();

			while (true)
			{
				try
				{
					using (StreamReader reader = new StreamReader(address))
					{
						XmlHelper helper = new XmlHelper(reader.BaseStream);
						dictionaryBuffer = helper.Parse();
					}

					break;
				}
				catch (Exception e)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine(e.Message);
					if (e is FileNotFoundException)
						Console.WriteLine("File not found");
					Console.ForegroundColor = ConsoleColor.Green;
				}
			}
		}

		private static void SftpWrite()
		{
			JsonHelper helper = new JsonHelper("config.json");
			Type type = Assembly.LoadFrom(helper.GetProperty("SftpClientAddress").Value<string>()).GetType("SftpClient_for_LigouniDictionary.Interaction");
			type.GetMethod("Write").Invoke(Activator.CreateInstance(type), new object[] { File.ReadAllBytes("temp.xml")});
		}

		private static void massiveModificationMode()
		{
			Console.ForegroundColor = ConsoleColor.Blue;
			string input = Console.ReadLine().ToLower();
			Console.ForegroundColor = ConsoleColor.Green;
			if (input != "startwrite") return;
			Console.WriteLine("Massive Modification Mode Activatied");

			while (true)
			{
				input = Console.ReadLine();
				if (input == "endwrite") break;

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
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}
			}

			try
			{
				XmlHelper.WriteToXml(dictionaryBuffer);
				SftpWrite();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
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

			foreach (var item in Prefix) stringBuilder.Append(new StringBuilder(item));
			foreach (var item in stem) stringBuilder.Append(new StringBuilder(item));
			foreach (var item in Suffix) stringBuilder.Append(new StringBuilder(item));

			CompleteWord = stringBuilder.ToString();
		}
	}

	class XmlHelper
	{
		public XmlDocument data { get; set; } = new XmlDocument();

		public XmlHelper(Stream stream) => data.Load(stream);

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
			if (uri == "") uri = "temp.xml";
			using (FileStream fs = File.Create(uri))
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

	class JsonHelper : IDisposable
	{
		string filepath;
		JObject jObject;

		public JsonHelper(string path)
		{
			filepath = path;
			using (StreamReader reader = new StreamReader(filepath))
			{
				string json = reader.ReadToEnd();
				jObject = JObject.Parse(json);
			}
		}

		public void Dispose() => File.WriteAllText(filepath, jObject.ToString());

		public JToken GetProperty(string name) => jObject.Property(name).Value;

		public void SetProperty<T>(string name, JToken value) => jObject.Property(name).Value = value;

		public void AddProperty(string name, JToken value) => jObject.Add(name, value);

		public IEnumerable<JProperty> ListProperties() => jObject.Properties();

		public bool Has(string propertyName) => jObject.Properties().Where(p => p.Name == propertyName).Count() != 0;
	}
}
