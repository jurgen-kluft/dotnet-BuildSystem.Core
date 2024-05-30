using System;

namespace GameCore
{
    /// <summary>
    /// Filename pattern matcher
    /// 
    /// This helper class matches filename patterns, also known as filenames with
    /// wildcards. This is a very simple form of expression parsing. The filename
    /// pattern understands only two wildcards, '?' and '*'. The '?' symbol matches
    /// any single character, '*' matches zero to an infinite amount of any characters.
    /// 
    /// Although this class is built to match filenames, it can be used to match
    /// any string of characters.
    /// </summary>
    public class FilenamePattern
    {
        private bool mIgnoreCase = false;
        private string mPattern = "*.*";

        public FilenamePattern()
        {
        }

        public FilenamePattern(string inPattern)
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

        public bool Matches(Filename inFilename)
        {
            var filename = inFilename.Name;

            var patternCharIndex = 0;
            var filenameCharIndex = 0;

            // Scan until first '*' in pattern
            while (filenameCharIndex < filename.Length && (mPattern[patternCharIndex] != '*'))
            {
                var p = mPattern[patternCharIndex];
                var c = filename[filenameCharIndex];
                if (mIgnoreCase)
                {
                    p = char.ToLower(p);
                    c = char.ToLower(c);
                }

                if ((p != c) && (p != '?'))
                    return false;

                patternCharIndex++;
                filenameCharIndex++;
            }

            var currentPatternCharIndex = 0;
            var currentFilenameCharIndex = 0;
            while (filenameCharIndex < filename.Length && patternCharIndex < mPattern.Length)
            {
                var p = mPattern[patternCharIndex];
                var c = filename[filenameCharIndex];
                if (mIgnoreCase)
                {
                    p = char.ToLower(p);
                    c = char.ToLower(c);
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