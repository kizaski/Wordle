using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace WordleLib
{
    public class Wordle
    {
        public enum MatchType
        {
            Exact = 0,
            Somewhere,
            Nowhere,
            Unknown
        }

        private static Random rnd = new Random();
        private string Secret;
        public int WordLength { get; private set; }
        public int AllowedTries { get; private set; }
        public List<string> Wordlist { get; set; }
        public List<List<MatchType>> Matches { get; private set; }
        public Dictionary<char, MatchType> Keyset_state { get; private set; }
        public List<string> Attempts { get; private set; }

        public bool IsAttemptTooShort { get; private set; }
        public bool IsAttemptNotFound { get; private set; }
        public bool IsWin { get; private set; }
        public bool IsOver { get; private set; }

        public event Action<string> WordNotFound;
        public event Action<string> WordTooShort;
        public event Action GameWon;
        public event Action<string> GameLost;

        /// <summary>
        ///     Constructor. Initialises game data
        /// </summary>
        /// <param name="wordLength">Length of the input word</param>
        /// <param name="keyset">Characters of which the words are comprised</param>
        public Wordle(List<string> wordlist,
                      string keyset,
                      int wordLength = 5,
                      int allowedTries = 6)
        {
            if (string.IsNullOrEmpty(keyset))
            {
                throw new ArgumentException($"'{nameof(keyset)}' cannot be null or empty.", nameof(keyset));
            }
            if (wordlist == null || !wordlist.Any())
            {
                throw new ArgumentException($"'{nameof(wordlist)}' cannot be null or empty.", nameof(wordlist));
            }

            WordLength = wordLength;
            AllowedTries = allowedTries;
            Wordlist = wordlist.FindAll(x => x.Length == WordLength);
            
            Matches = new List<List<MatchType>>();
            Keyset_state = new Dictionary<char, MatchType>(keyset.Length);
            Attempts = new List<string>(AllowedTries);

            for (int i = 0; i < AllowedTries; i++)
            {
                Attempts.Add("");
            }

            for (int i = 0; i < keyset.Length; i++)
            {
                Keyset_state.Add(char.ToUpperInvariant(keyset[i]), MatchType.Unknown);
            }

            GenerateSecret();
        }

        private void GenerateSecret()
        {
            int index = rnd.Next(0, Wordlist.Count);
            Secret = Wordlist.ElementAt(index);
        }

        public void TryWord(string attempt)
        {

            if (attempt.Length < WordLength || Attempts.Last().Length > WordLength)
            {
                IsAttemptTooShort = true;
                WordTooShort?.Invoke($"word length must be {WordLength} letters");

                return;
            }
            if (!Wordlist.Contains(attempt))
            {
                IsAttemptNotFound = true;
                WordNotFound?.Invoke("word is not found in the word list");

                return;
            }

            IsAttemptNotFound = false;
            IsAttemptTooShort = false;

            Attempts.Add(attempt);
            Matches.Add(new List<MatchType>());

            for (int i = 0; i < Secret.Length; i++)
            {
                var secret_char = char.ToUpperInvariant(Secret.ElementAt(i));
                var attemt_char = char.ToUpperInvariant(Attempts.Last().ElementAt(i));
                if (secret_char == attemt_char)
                {
                    Matches.Last().Add(MatchType.Exact);
                    Keyset_state[attemt_char] = MatchType.Exact;
                }
                else if (Secret.ToUpperInvariant().Contains(attemt_char))
                {
                    Matches.Last().Add(MatchType.Somewhere);
                    Keyset_state[attemt_char] = MatchType.Somewhere;
                }
                else
                {
                    Matches.Last().Add(MatchType.Nowhere);
                    Keyset_state[attemt_char] = MatchType.Nowhere;
                }
            }

            if (Matches.Last().All(x => x == MatchType.Exact))
            {
                IsOver = true;
                IsWin = true;
                GameWon?.Invoke();
            }
            else if (Matches.Count == AllowedTries)
            {
                IsOver = true;
                IsWin = false;
                GameLost?.Invoke(Secret);
            }

            return;
        }

        public void Reset()
        {
            for (int i = 0; i < Attempts.Count; i++)
            {
                Attempts[i] = "";
            }

            Matches.Clear();

            foreach(var k in Keyset_state)
            {
                Keyset_state[k.Key] = MatchType.Unknown;
            }

            IsAttemptNotFound = false;
            IsAttemptTooShort = false;
            IsOver = false;
            IsWin = false;

            GenerateSecret();
        }
    }
}