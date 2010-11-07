using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Mercury.App.Matching
{

    /// <summary>
    /// Normalizes item titles to prepare them for the processing by the substring matching algorithm
    /// </summary>
    public static class Normalizer
    {
        public const String WORD_SEPARATOR = "//";
        const String SPACE=" ";

        static Regex _nonPrintables;
        static Regex _punctuation;
        static Regex _upperLowerTransition;
        static Regex _alphaNonAlphaTransition;
        static Regex _nonAlphaAlphaTransition;
        static Regex _whitespace;

        static Normalizer() {
            PrecompileRegexes();
        }

        public static String Normalize(String title)
        {
            StringBuilder normTitle = new StringBuilder(title);

            ReplaceNonPrintableWithSpace(normTitle);
            ReplacePunctuationWithSpace(normTitle);
            MarkUpperLowerTransitionsWithSpace(normTitle);
            MarkAlphaNonAlphaTransitionsWithSpace(normTitle);
            RemoveLeadingAndTrailingSpaces(normTitle);
            CollapseWhitespaceIntoSeparator(normTitle);
            ConvertToLowerCase(normTitle);

            return normTitle.ToString();
        }

        private static void ReplaceNonPrintableWithSpace(StringBuilder normTitle)
        {
            ApplyRegex(normTitle, _nonPrintables, SPACE);
        }

        private static void ReplacePunctuationWithSpace(StringBuilder normTitle)
        {
            ApplyRegex(normTitle, _punctuation, SPACE);
        }

        private static void MarkUpperLowerTransitionsWithSpace(StringBuilder normTitle)
        {
            ApplyRegex(normTitle, _upperLowerTransition, "${first} ${second}");
        }

        private static void MarkAlphaNonAlphaTransitionsWithSpace(StringBuilder normTitle)
        {
            ApplyRegex(normTitle, _alphaNonAlphaTransition, "${first} ${second}");
            ApplyRegex(normTitle, _nonAlphaAlphaTransition, "${first} ${second}");
        }

        private static void RemoveLeadingAndTrailingSpaces(StringBuilder normTitle)
        {
            String trimmed = normTitle.ToString().Trim();
            normTitle.Remove(0, normTitle.Length);
            normTitle.Append(trimmed);
        }

        private static void CollapseWhitespaceIntoSeparator(StringBuilder normTitle)
        {
            ApplyRegex(normTitle, _whitespace, WORD_SEPARATOR);
        }

        private static void ConvertToLowerCase(StringBuilder normTitle)
        {
            String lower = normTitle.ToString().ToLower();
            normTitle.Remove(0, normTitle.Length);
            normTitle.Append(lower);
        }

        private static void ApplyRegex(StringBuilder normTitle,Regex expr,string replace)
        {
            String output = expr.Replace(normTitle.ToString(), replace);
            normTitle.Remove(0, normTitle.Length);
            normTitle.Append(output);
        }

        private static void PrecompileRegexes()
        {
            _nonPrintables = new Regex(@"\p{Cc}+", RegexOptions.Compiled);
            _punctuation = new Regex(@"(?x)
(?:
    (\p{Sm}) | (?# MathSymbol )
    (\p{Pe}) | (?# ClosePunctuation )
    (\p{Pc}) | (?# ConnectorPunctuation )
    (\p{Pd}) | (?# DashPunctuation )
    (\p{Pf}) | (?# FinalQuotePunctuation )
    (\p{Pi}) | (?# InitialQuotePunctuation )
    (\p{Ps}) | (?# OpenPunctuation )
    (\p{Po})   (?# OtherPunctuation )
)+", 
                RegexOptions.Compiled);

            _upperLowerTransition = new Regex(@"(?x)
(?:
    (?# Must use named groups, since ordinals will count up past the | operator )
    (?<first>\p{Ll})(?<second>\p{Lu}) | (?# LowercaseLetter to UppercaseLetter )
    (?<first>\p{Ll})(?<second>\p{Lt}) | (?# LowercaseLetter to TitlecaseLetter )
    (?<first>\p{Ll})(?<second>\p{Lo}) | (?# LowercaseLetter to OtherLetter )
    (?<first>\p{Lo})(?<second>\p{Lu}) | (?# OtherLetter to UppercaseLetter )
    (?<first>\p{Lo})(?<second>\p{Lt})   (?# OtherLetter to TitlecaseLetter )
)+",
                RegexOptions.Compiled);

            _alphaNonAlphaTransition = new Regex(@"(?x)
(?:
    (?# [\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Lm}] includes all the unicode Letter classes and nothing else, unlike
        \w, which also includes numbers and connector punctuation )
    (?<first>[\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Lm}]) (?<second>[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Lm}])(?# Word character to non-word character )
)+",
                RegexOptions.Compiled);

            //Since an alpha-nonalpha-alpha transition can overlap 
            //(eg, a2a is an alpha->nonalpha transition "a2" and nonalpha->alpha "2a"), 
            //two expressions must be used
            _nonAlphaAlphaTransition = new Regex(@"(?x)
(?:
    (?<first>[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Lm}])(?<second>[\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Lm}])    (?# non-word to word character )
)+",
                RegexOptions.Compiled);

            _whitespace = new Regex(@"\s+", RegexOptions.Compiled);
        }
    }
}
