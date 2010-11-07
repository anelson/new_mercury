using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework;

using Mercury.App;
using Mercury.App.Matching;

namespace Mercury.App.Tests.Matching
{
    [TestFixture]
    public class NormalizerTests
    {
        static String[,] KNOWN_ANSWER_TESTS = new String[,] {
            {"", ""},
            {"   ", ""},
            {"\t", ""},
            {"lword", "lword"},
            {"UWORD", "uword"},
            {"two words", "two#{WS}words"},
            {"CamelCaseWords", "camel#{WS}case#{WS}words"},
            {"CamelCaseWords", "camel#{WS}case#{WS}words"},
            {"null\x1value", "null#{WS}value"},
            {"punc+as!sep_char", "punc#{WS}as#{WS}sep#{WS}char"},
            {"  leading space  ", "leading#{WS}space"},
            {"alpha2nonalpha", "alpha#{WS}2#{WS}nonalpha"},
            {"dan_pdf_test", "dan#{WS}pdf#{WS}test"},
            {"latin and دعا رئيس الوزراء العراقي نوري", "latin#{WS}and#{WS}دعا#{WS}رئيس#{WS}الوزراء#{WS}العراقي#{WS}نوري"},
            {"lowerدعاUpper", "lower#{WS}دعا#{WS}upper"}
        };

        [Test]
        public void KnownAnswerTests()
        {
            for (int idx = 0; idx < KNOWN_ANSWER_TESTS.GetLength(0); idx++)
            {
                String input = KNOWN_ANSWER_TESTS[idx, 0];
                String expectedOutput = KNOWN_ANSWER_TESTS[idx, 1].Replace("#{WS}", Normalizer.WORD_SEPARATOR);

                Assert.AreEqual(expectedOutput, Normalizer.Normalize(input));
            }
        }
    }
}
