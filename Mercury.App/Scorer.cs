using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Mercury.App
{
    /// <summary>
    /// Flags which indicate how a search char appears in the title
    /// </summary>
    [Flags]
    public enum SearchCharFlags
    {
        None = 0,
        WordBoundary = 0x01,
        PreceedingChar = 0x02,
        SucceedingChar = 0x04,
        SurroundedChar = (PreceedingChar | SucceedingChar)
    };

    /// <summary>
    /// Given a title and a search string composed of ordered subsets of the title, computes a numeric score indicating
    /// how strongly the search string resembles a valid abbreviated form of the title.
    /// </summary>
    public static class Scorer
    {
        /// <summary>
        /// Computes a match score representing the liklihood (not in the probablistic sense) that searchString is
        /// a human-generated abbreviation of title.
        /// 
        /// title MUST contain all the characters in searchString, in the order in which they appear, though they 
        /// needn't be contiguous.  For example, caling Score with a title of 'This Sucks' and a search string is 'tss'
        /// is valid, since title contains a 't', an 's', and another 's, if not contiguously.  By contrast, a search string
        /// of 'sst' would not be valid, since no 't' follows two 's' characters in the title.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="searchString"></param>
        /// <returns>A match score between 0 and 1</returns>
        public static decimal Score(String title, String searchString)
        {
            if (title.Length == 0 || searchString.Length == 0)
            {
                throw new ArgumentException("title and searchString must be non-empty strings");
            }

            int[] searchCharPositions;
            SearchCharFlags[] searchCharFlags;

            FindSearchCharsInTitle(title, searchString, out searchCharPositions, out searchCharFlags);

            return SimpleScore(searchCharPositions, searchCharFlags);
        }

        private static void FindSearchCharsInTitle(string title, string searchString, out int[] searchCharPositions, out SearchCharFlags[] searchCharFlags)
        {
            //For each search character, find its right-most occurrence in title while preserving the search character order
            searchCharPositions = FindRightmostCharPositions(title, searchString);

            MarkWordBoundaries(title, searchString, searchCharPositions, out searchCharFlags);
        }

        public static int[] FindRightmostCharPositions(string title, string searchString)
        {
            int[] searchCharPositions = new int[searchString.Length];

            //Starting with the right-most search character and the end of the title,
            //working backwards find the right-most possible occurrence of each search character
            //in the title
            int prevPos = title.Length - 1;

            for (int idx = searchString.Length - 1; idx >= 0; idx--)
            {
                searchCharPositions[idx] = title.LastIndexOf(searchString[idx], prevPos);

                if (searchCharPositions[idx] == -1)
                {
                    throw new ArgumentException(String.Format("The search character '{0}' not found by \"{1}\".LastIndexOf('{0}', {2})",
                        searchString[idx],
                        title,
                        prevPos));
                }

                prevPos = searchCharPositions[idx] - 1;
            }

            return searchCharPositions;
        }

        public static void MarkWordBoundaries(string title, string searchString, int[] searchCharPositions, out SearchCharFlags[] searchCharFlags)
        {
            //  Check for search letters at the start of words, at positions equal to or earlier than
            //  the right-most positions found above
            int[] wordStartPositions;
            Char[] wordStartCharacters;
            
            searchCharFlags = new SearchCharFlags[searchString.Length];

            FindWordStarts(title, out wordStartPositions, out wordStartCharacters);
            List<Char> wordStartCharsList = new List<char>(wordStartCharacters);

            int wordIdx = 0;

            for (int idx = 0; idx < searchCharPositions.Length; idx++)
            {
                int val = searchCharPositions[idx];
                Char searchChar = searchString[idx];

                // Does the character at this index start a word?
                int tempIdx = wordStartCharsList.IndexOf(searchChar, wordIdx);

                if (tempIdx != -1 &&
                    wordStartPositions[tempIdx] <= val)
                {
                    // A word starting with this character is present in the string on or before
                    // the right-most occurence of this char.
                    searchCharFlags[idx] = SearchCharFlags.WordBoundary;
                    wordIdx = tempIdx + 1;

                    if (wordIdx == wordStartCharacters.Length)
                    {
                        // No more words
                        break;
                    }
                }
                else
                {
                    // No word starting with this character.  pick up the word-start search for subsequent
                    // characters, after the nearest occurence of this character.  
                    int nearestCharIdx = title.IndexOf(searchString[idx], wordStartPositions[wordIdx]);

                    if (nearestCharIdx == -1)
                    {
                        // This character isn't present past the current word, which means no further
                        // characters will be either; abort the search
                        break;
                    }

                    // Find the first word start index AFTER this char
                    wordIdx = -1;

                    for (int wordStartPosIdx = 0;
                        wordStartPosIdx < wordStartPositions.Length;
                        wordStartPosIdx++)
                    {
                        int wordStartPos = wordStartPositions[wordStartPosIdx];

                        if (wordStartPos > nearestCharIdx)
                        {
                            wordIdx = wordStartPosIdx;
                        }
                    }

                    if (wordIdx == -1)
                    {
                        // No more word starts; that's the last of the search terms that align on
                        // word boundaries
                        break;
                    }
                }
            }
        }

        public static void FindWordStarts(String title, out int[] wordStartPositions, out Char[] wordStartCharacters)
        {
            List<int> wordStartPosList = new List<int>();
            List<Char> wordStartCharsList = new List<char>();

            int matchOffset = 0;
            
            Regex nextWordRegEx = new Regex(Regex.Escape(Matching.Normalizer.WORD_SEPARATOR) + "(.)");

            //Match the first letter in the title to start with.
            Match match = Regex.Match(title, "^(.)");
            while (match.Success)
            {
                //Group 0 is the entire regex; group 1 is the first expression tagged with ()
                Group matchGroup = match.Groups[1];

                // The position of this match, relative to the start of the title
                wordStartPosList.Add(matchGroup.Index);
                wordStartCharsList.Add(matchGroup.Value[0]);

                //Get the next word start, in the form of a word separator followed by a character
                matchOffset = matchGroup.Index + matchGroup.Length;
                match = nextWordRegEx.Match(title, matchOffset);
            }

            wordStartPositions = wordStartPosList.ToArray();
            wordStartCharacters = wordStartCharsList.ToArray();
        }

        private static decimal SimpleScore(int[] searchCharPositions, SearchCharFlags[] searchCharFlags)
        {
            double maxPerCharScore = 1.0/(double)searchCharPositions.Length;

            double totalScore = 0;

            foreach (SearchCharFlags flags in searchCharFlags) {
                if ((flags & SearchCharFlags.WordBoundary) != 0 ||
                    (flags & SearchCharFlags.SurroundedChar) != 0) {
                    //This character is a word boundary, or is surrounded by matching search chars on both sides
                    totalScore += maxPerCharScore;
                } else if ((flags & SearchCharFlags.PreceedingChar) != 0 ||
                    (flags & SearchCharFlags.SucceedingChar) != 0) {
                    //Preceeds or succeeds a matching char; that's only a half score
                    totalScore += maxPerCharScore / 2.0;
                }
            }

            return (decimal)totalScore;
        }
    }
}
