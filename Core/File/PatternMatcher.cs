using System;

namespace Core
{
    /// <summary>
    /// Pattern matcher
    /// 
    /// This helper class matches filename patterns, also known as filenames with
    /// wildcards. This is a very simple form of expression parsing. The filename
    /// pattern understands only two wildcards, '?' and '*'. The '?' symbol matches
    /// any single character, '*' matches zero to an infinite amount of any characters.
    /// 
    /// Although this class is built to match filenames, it can be used to match
    /// any string of characters.
    /// </summary>
    public class PatternMatcher
    {
        private bool mIgnoreCase = false;
        private string mPattern = "*.*";

        public PatternMatcher()
        {
        }

        public PatternMatcher(string inPattern)
        {
            mPattern = inPattern;
        }

        public bool IgnoreCase
        {
            get
            {
                return mIgnoreCase;
            }
            set
            {
                mIgnoreCase = value;
            }
        }

        public string Pattern
        {
            get
            {
                return mPattern;
            }
            set
            {
                mPattern = value;
            }
        }

        public bool Matches(string inString)
        {
            string filename = inString;

            int patternCharIndex = 0;
            int filenameCharIndex = 0;

            // Scan until first '*' in pattern
            while (filenameCharIndex < filename.Length && (mPattern[patternCharIndex] != '*'))
            {
                char p = mPattern[patternCharIndex];
                char c = filename[filenameCharIndex];
                if (mIgnoreCase)
                {
                    p = Char.ToLower(p);
                    c = Char.ToLower(c);
                }

                if ((p != c) && (p != '?'))
                    return false;

                patternCharIndex++;
                filenameCharIndex++;
            }

            int currentPatternCharIndex = 0;
            int currentFilenameCharIndex = 0;
            while (filenameCharIndex < filename.Length && patternCharIndex < mPattern.Length)
            {
                char p = mPattern[patternCharIndex];
                char c = filename[filenameCharIndex];
                if (mIgnoreCase)
                {
                    p = Char.ToLower(p);
                    c = Char.ToLower(c);
                }

                if (p == '*')
                {
                    ++patternCharIndex;
                    if (patternCharIndex == mPattern.Length)
                        return true;

                    currentPatternCharIndex = patternCharIndex;
                    currentFilenameCharIndex = filenameCharIndex + 1;
                }
                else if ((p == c) || (p == '?'))
                {
                    patternCharIndex++;
                    filenameCharIndex++;
                }
                else if (p == 0)
                {
                    break;
                }
                else
                {
                    patternCharIndex = currentPatternCharIndex;
                    filenameCharIndex = currentFilenameCharIndex++;
                }

            }

            while (patternCharIndex < mPattern.Length && mPattern[patternCharIndex] == '*')
                patternCharIndex++;

            return patternCharIndex == mPattern.Length && filenameCharIndex == filename.Length;
        }
    }
}