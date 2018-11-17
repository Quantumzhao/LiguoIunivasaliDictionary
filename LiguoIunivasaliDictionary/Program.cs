using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LiguoIunivasaliDictionary
{
	class Program
	{
		static List<Vocabulary> dictionaryBuffer = new List<Vocabulary>();

		static void Main(string[] args)
		{
			Initiate();

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

							dictionaryBuffer.Add
							(
								new Vocabulary
								{
									Vocab = tempItem[0],
									Meaning = tempItem[1]
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

				Console.WriteLine($"{word.Meaning}\n");
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
			foreach (var item in dictionaryBuffer)
			{
				Console.WriteLine("{0, -20}{1}", item.Vocab, item.Meaning);
			}

			Console.WriteLine("\n\n");
		}
	}

	class Vocabulary
	{
		public string Vocab { get; set; }

		public string Meaning { get; set; }
	}
}
