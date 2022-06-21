using System;
using System.IO;
using System.Linq;
using WordleLib;

namespace ConsoleWordle
{
    class Program
    {
        static void Main(string[] args)
        {

            string words = "";
            var sr = new StreamReader(@"../../../res/words.txt");
            using (sr)
            {
                words = sr.ReadToEnd();
            }

            Wordle wordle = new Wordle(wordlist: words.Split("\r\n").ToList(),
                                       keyset: "ABCDEFGHIJKLMNOPQRSTUVWXYZ"); // АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЬЮЯ

            wordle.WordNotFound += mes => Console.WriteLine(mes);
            wordle.WordTooShort += mes => Console.WriteLine(mes);
            wordle.GameWon += () => Console.WriteLine("Congratulations, you won.");
            wordle.GameLost += x => Console.WriteLine($"You lost! Word was {x}");

            Console.WriteLine("\nWordle");
            Console.WriteLine("C - letter is at the correct place");
            Console.WriteLine("S - letter is somewhere else");
            Console.WriteLine("n - letter is not in the word");
            Console.WriteLine("Enter a word\n");

            while (!wordle.IsOver)
            {
                wordle.TryWord(Console.ReadLine());

                if (wordle.Matches.Count > 0 && !wordle.IsAttemptTooShort && !wordle.IsAttemptNotFound && !wordle.IsOver)
                {
                    wordle.Matches.Last().ForEach(x =>
                    {
                        switch (x)
                        {
                            case Wordle.MatchType.Exact:
                                Console.Write("C");
                                break;
                            case Wordle.MatchType.Somewhere:
                                Console.Write("S");
                                break;
                            case Wordle.MatchType.Nowhere:
                                Console.Write("n");
                                break;
                            default:
                                break;
                        }
                    });
                }
                Console.WriteLine();
                Console.WriteLine();
            }
        }
    }
}
