using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace LiguoIunivasaliDictionary
{
	class Program
	{
		static List<Vocabulary> dictionaryBuffer = new List<Vocabulary>();
		static XmlHelper XmlHelper { get; set; } = new XmlHelper("Dictionary.xml");

		static void Main(string[] args)
		{
			Initiate();
			XmlHelper.Parse();

			while (true)
			{
				Console.WriteLine("Please Choose an Action: \n" +
					"    [1]Find the Definition of a Word\n" +
					"    [2]Find the Word in LigoUni\n" +
					"    [3]Add a new Word to Dictionary\n" +
					"    [4]Print the dictionary on Screen\n" + 
					"    [5]Exit\n");

				ConsoleKey key = Console.ReadKey().Key;

				Console.WriteLine("\n\n");

				switch (key)
				{
					case ConsoleKey.D1:
						FindDef();
						break;

					case ConsoleKey.D2:
						FindWord();
						break;

					case ConsoleKey.D3:
						AddDef();
						break;

					case ConsoleKey.D4:
						PrintDictionary();
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

			try
			{
				using (StreamReader reader = new StreamReader(@"C:\Users\yisha\OneDrive\Documents\LiguoIunivasaliDictionary.ini"))
				{
					while (!reader.EndOfStream)
					{
						try
						{
							string[] tempItem = reader.ReadLine().Split('|');

							Word tempWord = new Word();
							tempWord.Morpheme.Add(new Snippet {Color = ConsoleColor.Green});

							char[] tempCharArray = tempItem[0].ToCharArray();

							for (int i = 0; i < tempCharArray.Length; i++)
							{
								switch (tempCharArray[i])
								{
									case '*':
										tempWord.Morpheme.Add
										(
											new Snippet
											{
												Color = ConsoleColor.Blue
											}
										);
										break;

									case '\'':
										tempWord.Morpheme.Add
										(
											new Snippet {
												Color = i % 2 == 0 ? 
												ConsoleColor.Green : ConsoleColor.DarkGreen
											}
										);										
										break;

									case '-':
										tempWord.Morpheme.Add
										(
											new Snippet
											{
												Color = ConsoleColor.DarkCyan
											}
										);
										break;

									default:
										tempWord.Morpheme[tempWord.Morpheme.Count - 1].Name.Append(tempCharArray[i]);
										break;
								}
							}

							dictionaryBuffer.Add
							(
								new Vocabulary
								{
									Vocab = tempWord,
									Definition = tempItem[1]
								}
							);
						}
						catch (IndexOutOfRangeException)
						{
							Console.ForegroundColor = ConsoleColor.Yellow;
							Console.WriteLine("There is something wrong when reading the dictionary...");
							Console.ForegroundColor = ConsoleColor.Green;
						}
					}
				}

				if (dictionaryBuffer.Count == 0)
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("Warning: The Requesting Dictionary is empty. ");
					Console.ForegroundColor = ConsoleColor.Green;
				}
			}
			catch (IOException ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"File dos not exist or moved. \n{ex.Message}");
				Console.ForegroundColor = ConsoleColor.Green;
			}
			catch(Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Unknown Error. \n{ex.Message}");
				Console.ForegroundColor = ConsoleColor.Green;
			}
		}

		static void FindDef()
		{
			Console.WriteLine("Please Enter Your Querry");

			string input = Console.ReadLine();

			var list = from item in dictionaryBuffer
					   where item.Vocab.Equals(input)
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

		static void FindWord()
		{
			Console.WriteLine("Function not implemented yet! \n");
		}

		static void AddDef()
		{
			Console.WriteLine("Function not implemented yet! \n");
		}

		static void PrintDictionary()
		{
			foreach (var vocabulary in dictionaryBuffer)
			{
				foreach (var item in vocabulary.Vocab.Morpheme)
				{
					Console.ForegroundColor = item.Color;
					Console.Write(item.Name);
				}

				Console.ForegroundColor = ConsoleColor.Green;

				Console.CursorLeft = 20;

				Console.Write(vocabulary.Definition + "\n");
			}

			Console.WriteLine("\n\n");
		}
	}

	class Vocabulary
	{
		public Word Vocab { get; set; }

		public string Definition { get; set; }
	}

	class Word
	{
		public List<Snippet> Morpheme { get; set; } = new List<Snippet>();

		public List<string> Prefix { get; set; } = new List<string>();
		public List<string> Root { get; set; } = new List<string>();
		public List<string> Suffix { get; set; } = new List<string>();
	}
	
	class Snippet
	{
		public StringBuilder Name { get; set; } = new StringBuilder();

		public ConsoleColor Color { get; set; }
	}

	class XmlHelper
	{
		XmlDocument data = new XmlDocument();

		public XmlHelper(string address)
		{
			ReadToMemory(address);
		}

		public void ReadToMemory(string address)
		{
			data.Load(address);
		}

		public List<Vocabulary> Parse()
		{
			XmlNode dictionary = data["dictionary"];

			List<Vocabulary> vocabularies = new List<Vocabulary>();

			foreach (XmlNode xmlVocabulary in dictionary)
			{
				XmlNode xmlWord = xmlVocabulary.SelectSingleNode("word");
				Word word = new Word();

				word.Prefix = xmlWord["prefix"].InnerText.Split().ToList();
				word.Root = xmlWord["morpheme"].InnerText.Split().ToList();
				word.Suffix = xmlWord["suffix"].InnerText.Split().ToList();

				string definition = xmlVocabulary.SelectSingleNode("definition").InnerText;

				vocabularies.Add(new Vocabulary() { Vocab = word, Definition = definition});
			}

			return vocabularies;
		}
	}
}
