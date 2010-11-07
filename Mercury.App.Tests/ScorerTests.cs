using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

using Mercury.App;
using Mercury.App.Matching;

namespace Mercury.App.Tests
{
    [TestFixture]
    public class ScorerTests
    {
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void EmptyBothTest()
        {
            Scorer.Score(String.Empty, String.Empty);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void EmptyTitleTest()
        {
            Scorer.Score("test", String.Empty);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void EmptySearchTest()
        {
            Scorer.Score(String.Empty, "test");
        }

        [Test]
        public void FindRightmostCharPositionsTest()
        {
            String title = "abcdefgfoobarbaz";
            String search = "afaz";

            int[] searchPositions = Scorer.FindRightmostCharPositions(title, search);

            Assert.AreEqual(title.Length - 1, searchPositions[3]);
            Assert.AreEqual(title.Length - 2, searchPositions[2]);
            Assert.AreEqual(title.Length - 9, searchPositions[1]);
            Assert.AreEqual(0, searchPositions[0]);
        }

        [Test]
        public void FindWordStartsTest()
        {
            String title = String.Format("fear{0}is{0}the{0}mind{0}killer", Normalizer.WORD_SEPARATOR);

            int[] wordStartPositions;
            Char[] wordStartCharacters;

            Scorer.FindWordStarts(title, out wordStartPositions, out wordStartCharacters);

            Assert.AreEqual(5, wordStartPositions.Length);
            Assert.AreEqual(5, wordStartCharacters.Length);

            Assert.AreEqual('f', wordStartCharacters[0]);
            Assert.AreEqual(wordStartCharacters[0], title[wordStartPositions[0]]);

            Assert.AreEqual('i', wordStartCharacters[1]);
            Assert.AreEqual(wordStartCharacters[1], title[wordStartPositions[1]]);

            Assert.AreEqual('t', wordStartCharacters[2]);
            Assert.AreEqual(wordStartCharacters[2], title[wordStartPositions[2]]);

            Assert.AreEqual('m', wordStartCharacters[3]);
            Assert.AreEqual(wordStartCharacters[3], title[wordStartPositions[3]]);

            Assert.AreEqual('k', wordStartCharacters[4]);
            Assert.AreEqual(wordStartCharacters[4], title[wordStartPositions[4]]);
        }

        [Test]
        public void MarkWordBoundariesTest()
        {
            String title = String.Format("fear{0}is{0}the{0}mind{0}killer", Normalizer.WORD_SEPARATOR);
            String search = "fmik";

            int[] searchPositions = Scorer.FindRightmostCharPositions(title, search);
            SearchCharFlags[] searchCharFlags = new SearchCharFlags[searchPositions.Length];

            Scorer.MarkWordBoundaries(title, search, searchPositions, out searchCharFlags);

            Assert.AreEqual(SearchCharFlags.WordBoundary, searchCharFlags[0]);
            Assert.AreEqual(SearchCharFlags.WordBoundary, searchCharFlags[1]);
            Assert.AreEqual(SearchCharFlags.None, searchCharFlags[2]);
            Assert.AreEqual(SearchCharFlags.WordBoundary, searchCharFlags[3]);
        }

        [Test]
        public void AllStartWordsMatchTest()
        {
            String title = String.Format("fear{0}is{0}the{0}mind{0}killer", Normalizer.WORD_SEPARATOR);
            String search = "fitmk";

            Assert.AreEqual(1.0,
                          Scorer.Score(title, search));
        }

        [Test]
        public void NoStartWordsMatchTest()
        {
            String title = String.Format("fear{0}is{0}the{0}mind{0}killer", Normalizer.WORD_SEPARATOR);
            String search = "eshii";

            Assert.AreEqual(0,
                          Scorer.Score(title, search));
        }

        [Test]
        public void SomeStartWordsMatchTest()
        {
            String title = String.Format("fear{0}is{0}the{0}mind{0}killer", Normalizer.WORD_SEPARATOR);
            String search = "fitil";

            Assert.AreEqual(3.0 / 5.0,
                        Scorer.Score(title, search));
        }

        [Test]
        public void AmbiguousWordStartMatchTest()
        {
            String title = Normalizer.Normalize("FoobarOofBad");
            String search = "fob"; // Could match 'FOoB' or 'FoobarOofBad'; maximal score logic dictates the highest scoring possibility be used

            Assert.AreEqual(1.0,
                          Scorer.Score(title, search));
        }

        [Test]
        public void RealWorldTest()
        {
            String title = Normalizer.Normalize("dan_pdf_test");
            String search = "test";

            Assert.AreEqual(1.0, Scorer.Score(title, search));
        }

        [Test]
        public void ZeroScoreTest()
        {
            String title = Normalizer.Normalize("military.cheaperthandirt.com");
            String search = "nic";

            Assert.AreEqual(0.0, Scorer.Score(title, search));
        }

        [Test]
        public void MinAreaCoveredTest()
        {
            String title = "foobarbazboofoo";
            String search = "f";

            Assert.AreEqual(1.0,
                         Scorer.Score(title, search));
        }

        [Test]
        public void NormAreaCoveredTest()
        {
            String title = "foobarbazboofoo";
            String search = "foob";

            Assert.AreEqual(7.0 / 8.0,
                         Scorer.Score(title, search));
        }

        [Test]
        public void CompleteSubstringMatchSpreadTest()
        {
            String title = "onlytesting1234";
            String search = "test";

            Assert.AreEqual(3.0 / 4.0,
                         Scorer.Score(title, search));
        }

        [Test]
        public void IncompleteSubstringMatchSpreadTest()
        {
            String title = "onlytesting1234";
            String search = "o4";

            Assert.AreEqual(0.5,
                         Scorer.Score(title, search));
        }
    }
}
