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
				Console.WriteLine("Please Enter Querry: \n");

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
				catch (Exception)
				{
					Console.WriteLine("\nNo match Word");
				}
			}
		}

		static void Initiate()
		{
			Console.ForegroundColor = ConsoleColor.Green;

			using (StreamReader reader = new StreamReader(@"C:\Users\yisha\OneDrive\Documents\LiguoIunivasaliDictionary.ini"))
			{
				while (!reader.EndOfStream)
				{
					string[] tempItem = reader.ReadLine().Split('|');

					dictionaryBuffer.Add
					(
						new Vocabulary
						{
							Vocab = tempItem[0],
							Meaning = tempItem[1],
							Index = int.Parse(tempItem[2])
						}
					);
				}
			}
		}
	}

	class Vocabulary
	{
		public string Vocab { get; set; }

		public string Meaning { get; set; }

		public int Index { get; set; }
	}
}
