using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace GameCore
{
    #region String Formatting Documentation
    // <summary>
    // The {0} in the string above is replaced with the value of nError, but what
    // if you want to specify the number of digits to use? Or the base (hexadecimal etc)?
    // The framework supports all this, but where it seemd confusing is that it's not
    // the String.Format function that does the string formatting, but rather the types
    // themselves. Every object has a method called ToString that returns a string
    // representation of the object. The ToString method can accept a string parameter,
    // which tells the object how to format itself - in the String.Format call, the
    // formatting string is passed after the position, for example, "{0:##}"
    // </summary>
    //
    // The text inside the curly braces is {index[,alignment][:formatString]}. If
    // alignment is positive, the text is right-aligned in a field the given number
    // of spaces; if it's negative, it's left-aligned.
    //
    // Strings
    // There really isn't any formatting within a string beyond it's alignment. Alignment works
    // for any argument being printed in a String.Format call.
    //
    // Sample 										Generates
    // code
    //     String.Format(" ->{1,10}<-", "Hello");		| Hello|
    //	    String.Format(" ->{1,-10}<-", "Hello"); 	|Hello |
    // code
    //
    // Numbers
    //
    // Basic number formatting specifiers:
    //
    // Specifier 	Type 								Format 	Output (Passed Double 1.42) 	Output (Passed Int -12400)
    // c 			Currency							{0:c} 	$1.42 							-$12,400
    // d 			Decimal	(Whole number) 				{0:d} 	System.FormatException 			-12400
    // e 			Scientific 							{0:e} 	1.420000e+000 					-1.240000e+004
    // f 			Fixed point							{0:f} 	1.42 							-12400.00
    // g 			General								{0:g} 	1.42 							-12400
    // n 			Number with commas for thousands 	{0:n} 	1.42 							-12,400
    // r 			Round trippable 					{0:r} 	1.42 							System.FormatException
    // v 			Hexadecimal 						{0:x4} 	System.FormatException 			cf90
    //
    // Custom number formatting:
    //
    // Specifier 	Type 					Example 		Output (Passed Double 1500.42) 		Note
    // 0 			Zero placeholder 		{0:00.0000} 	1500.4200 							Pads with zeroes.
    // # 			Digit placeholder 		{0:(#).##} 		(1500).42
    // . 			Decimal point 			{0:0.0} 		1500.4
    // , 			Thousand separator 		{0:0,0} 		1,500 								Must be between two zeroes.
    // ,. 			Number scaling 			{0:0,.} 		2 									Comma adjacent to Period scales by 1000.
    // % 			Percent 				{0:0%} 			150042% 							Multiplies by 100, adds % sign.
    // e 			Exponent placeholder 	{0:00e+0} 		15e+2 								Many exponent formats available.
    // ; 			Group separator 	see below
    ///
    // The group separator is especially useful for formatting currency values which
    // require that negative values be enclosed in parentheses. This currency formatting
    // example at the bottom of this document makes it obvious:
    //
    // Dates
    //
    // Note that date formatting is especially dependant on the system's regional settings; the
    // example strings here are from my local locale.
    //
    // Specifier 	Type 								Example (Passed System.DateTime.Now)
    // d 			Short date 							10/12/2002
    // D 			Long date 							December 10, 2002
    // t 			Short time 							10:11 PM
    // T 			Long time 							10:11:29 PM
    // f 			Full date & time 					December 10, 2002 10:11 PM
    // F 			Full date & time (long) 			December 10, 2002 10:11:29 PM
    // g 			Default date & time 				10/12/2002 10:11 PM
    // G 			Default date & time (long) 			10/12/2002 10:11:29 PM
    // M 			Month day pattern 					December 10
    // r 			RFC1123 date string 				Tue, 10 Dec 2002 22:11:29 GMT
    // s 			Sortable date string 				2002-12-10T22:11:29
    // u 			Universal sortable, local time 		2002-12-10 22:13:50Z
    // U 			Universal sortable, GMT 			December 11, 2002 3:13:50 AM
    // Y 			Year month pattern 					December, 2002
    //
    // The 'U' specifier seems broken; that string certainly isn't sortable.
    //
    // Custom date formatting:
    // Specifier 		Type 						Example 		Example Output
    // dd 				Day 						{0:dd} 			10
    // ddd 			    Day name 					{0:ddd} 		Tue
    // dddd 			Full day name 				{0:dddd} 		Tuesday
    // f, ff, ... 	 	Second fractions 			{0:fff} 		932
    // gg, ...			Era							{0:gg} 			A.D.
    // hh 				2 digit hour 				{0:hh} 			10
    // HH 				2 digit hour, 24hr format 	{0:HH} 			22
    // mm 				Minute 00-59 				{0:mm} 			38
    // MM 				Month 01-12 				{0:MM} 			12
    // MMM 			    Month abbreviation 			{0:MMM} 		Dec
    // MMMM 			Full month name 			{0:MMMM} 		December
    // ss 				Seconds 00-59 				{0:ss} 			46
    // tt 				AM or PM 					{0:tt} 			PM
    // yy 				Year, 2 digits 				{0:yy} 			02
    // yyyy 			Year 						{0:yyyy} 		2002
    // zz 				Timezone offset, 2 digits 	{0:zz} 			-05
    // zzz 			    Full timezone offset 		{0:zzz} 		-05:00
    // : 				Separator 					{0:hh:mm:ss} 	10:43:20
    // / 				Separator 					{0:dd/MM/yyyy} 	10/12/2002
    //
    // Enumerations
    //
    // Specifier 	Type
    // g 			Default (Flag names if available, otherwise decimal)
    // f 			Flags always
    // d 			Integer always
    // v 			Eight digit hex.
    //
    // Some Useful Examples
    //
    // String.Format("{0:$#,##0.00;($#,##0.00);Zero}", value);
    //
    //     This will output "$1,240.00" if passed 1243.50. It will output the same
    //		format but in parentheses if the number is negative, and will output the
    //		string "Zero" if the number is zero.
    //
    // String.Format("{0:(###) ###-####}", 8005551212);
    //
    //     This will output "(800) 555-1212".
    //
    // </summary>

    #endregion

    // <summary>
    // UtilityClass with some additional functions for string operations
    // </summary>

    public static class StringTools
    {
        private static int CharToIndex(char c)
        {
            return c switch
            {
                >= '0' and <= '9' => 0 + (c - '0'),
                >= 'a' and <= 'z' => 2 + (c - 'a'),
                >= 'A' and <= 'Z' => 3 + (c - 'A'),
                '.' => 30,
                _ => 31
            };
        }

        public static int Encode_32_5(params char[] characters)
        {
            var h = 0;
            foreach (var c in characters)
            {
                var i = CharToIndex(c);
                h = (h << 5) | (i & 0x1F);
            }
            return h;
        }
        public static long Encode_64_13(params char[] characters)
        {
            var h = (long)0;
            foreach (var c in characters)
            {
                var i = (long)CharToIndex(c);
                h = (h << 5) | (i & 0x1F);
            }
            return h;
        }
        private static readonly Regex sNullStringRegex = new Regex("\0");

        public static bool Contains(string text, string fragment)
        {
            return text.IndexOf(fragment, StringComparison.Ordinal) > -1;
        }

        public static bool EqualsIgnoreCase(string a, string b)
        {
            return CaseInsensitiveComparer.Default.Compare(a, b) == 0;
        }

        public static string JoinUnique(string delimiter, params string[][] fragmentArrays)
        {
            var list = new SortedList();
            foreach (var fragmentArray in fragmentArrays)
            {
                foreach (var fragment in fragmentArray)
                {
                    if (!list.Contains(fragment))
                        list.Add(fragment, fragment);
                }
            }
            var buffer = new StringBuilder();
            foreach (string value in list.Values)
            {
                if (buffer.Length > 0)
                {
                    buffer.Append(delimiter);
                }
                buffer.Append(value);
            }
            return buffer.ToString();
        }

        public static int GenerateHashCode(params string[] values)
        {
            var hashcode = 0;
            foreach (var value in values)
            {
                if (value != null)
                {
                    hashcode += value.GetHashCode();
                }
            }
            return hashcode;
        }

        public static string LastWord(string input, string separators = " .,;!?:")
        {
            if (input == null)
            {
                return null;
            }
            var tokens = input.Split(separators.ToCharArray());
            for (var i = tokens.Length - 1; i >= 0; i--)
            {
                if (IsWhitespace(tokens[i]) == false)
                {
                    return tokens[i].Trim();
                }
            }
            return string.Empty;
        }

        public static bool IsBlank(string input)
        {
            return string.IsNullOrEmpty(input);
        }

        public static bool IsWhitespace(string input)
        {
            return (string.IsNullOrEmpty(input) || input.Trim().Length == 0);
        }

        public static string Strip(string input, params string[] removals)
        {
            var revised = input;
            foreach (var removal in removals)
            {
                var i = 0;
                while ((i = revised.IndexOf(removal, StringComparison.Ordinal)) > -1)
                {
                    revised = revised.Remove(i, removal.Length);
                }
            }
            return revised;
        }

        public static string[] Insert(string[] input, string insert, int index)
        {
            var list = new ArrayList(input);
            list.Insert(index, insert);
            return (string[])list.ToArray(typeof(string));
        }

        public static string Join(string separator, params string[] strings)
        {
            var builder = new StringBuilder();
            foreach (var s in strings)
            {
                if (IsBlank(s))
                    continue;
                if (builder.Length > 0)
                    builder.Append(separator);

                builder.Append(s);
            }
            return builder.ToString();
        }

        public static string RemoveNulls(string s)
        {
            return sNullStringRegex.Replace(s, string.Empty).TrimStart();
        }

        public static string StripQuotes(string filename)
        {
            return filename?.Trim('"');
        }

        public static string SurroundInQuotesIfContainsSpace(string value, string quote = "\"")
        {
            if (!IsBlank(value) && value.IndexOf(' ') >= 0)
                return string.Format(@"{0}{1}{0}", quote, value);

            return value;
        }

        public static bool IsNumber(char c)
        {
            return c is >= '0' and <= '9';
        }

        public static bool IsHexNumber(char c)
        {
            return c is >= '0' and <= '9' or >= 'A' and <= 'F' or >= 'a' and <= 'f';
        }

        public static bool IsDecimalNumber(string str)
        {
            foreach (var c in str)
            {
                if (!IsNumber(c))
                    return false;
            }
            return true;
        }

        public static byte HexToNibble(char c)
        {
            byte b = c switch
            {
                >= '0' and <= '9' => (byte)(c - '0'),
                >= 'a' and <= 'f' => (byte)(10 + (c - 'a')),
                >= 'A' and <= 'F' => (byte)(10 + (c - 'A')),
                _ => 0
            };
            return b;
        }

        public static int HexToInt32(string str)
        {
            if (str.StartsWith("0x"))
                str = str.Substring(2);

            var value = 0;
            foreach (var c in str)
            {
                if (value == 0 && c == '0')
                    continue;

                byte b = c switch
                {
                    >= '0' and <= '9' => (byte)(c - '0'),
                    >= 'a' and <= 'f' => (byte)(10 + (c - 'a')),
                    >= 'A' and <= 'F' => (byte)(10 + (c - 'A')),
                    _ => 0
                };
                value = value << 4;
                value = value | b;
            }
            return value;
        }

        public static long HexToInt64(string str)
        {
            if (str.StartsWith("0x"))
                str = str.Substring(2);

            long value = 0;
            foreach (var c in str)
            {
                if (value == 0 && c == '0')
                    continue;

                byte b = c switch
                {
                    >= '0' and <= '9' => (byte)(c - '0'),
                    >= 'a' and <= 'f' => (byte)(10 + (c - 'a')),
                    >= 'A' and <= 'F' => (byte)(10 + (c - 'A')),
                    _ => 0
                };
                value = value << 4;
                value = value | b;
            }
            return value;
        }

        public static char NibbleToHex(byte b)
        {
            return b switch
            {
                <= 9 => (char)('0' + b),
                >= 10 and <= 15 => (char)('A' - 10 + b),
                _ => '?'
            };
        }

        /// <summary>
        /// Checks if the string is of the form header('0x') + hexadecimal number
        /// </summary>
        /// <param name="str"></param>
        /// <returns>True if string contains a hexadecimal number</returns>
        public static bool IsHexadecimalNumber(string str, bool header)
        {
            var i = 0;
            foreach (var c in str)
            {
                if (header)
                {
                    switch (i)
                    {
                        case 0 when c != '0':
                            return false;
                        case 0:
                            continue;
                        case 1 when c != 'x':
                            return false;
                        case 1:
                            continue;
                    }
                }
                if (!IsHexNumber(c))
                {
                    return false;
                }

                ++i;
            }
            return true;
        }

        public static string MultiToSingle(string[] strings)
        {
            var str = string.Empty;
            foreach (var s in strings)
            {
                if (string.IsNullOrEmpty(str))
                {
                    str = s;
                }
                else
                {
                    str = str + System.Environment.NewLine + s;
                }
            }
            return str;
        }

        public static string MultiToSingle(string s, char charToReplace, char replacementChar)
        {
            var sb = new StringBuilder(s.Length);
            var skipped = false;
            foreach (var c in s)
            {
                if (c == charToReplace)
                {
                    skipped = true;
                }
                else
                {
                    if (skipped)
                    {
                        skipped = false;
                        sb.Append(replacementChar);
                    }

                    sb.Append(c);
                }
            }

            if (skipped)
                sb.Append(replacementChar);

            return sb.ToString();
        }

        public static string[] SingleToMulti(string strings)
        {
            var str = strings.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            return str;
        }

        public static string SingleToMulti(string s, char charToReplace, char replacementChar, int replacementCount)
        {
            var sb = new StringBuilder(s.Length);
            foreach (var c in s)
            {
                if (c == charToReplace)
                {
                    sb.Append(replacementChar, replacementCount);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Concatenate all the strings in array <paramref name="strings"/> and seperate them with <paramref name="delimiter"/>
        /// </summary>
        /// <param name="strings">The strings to concatenate.</param>
        /// <param name="delimiter">The delimiter to seperate the concatenated strings.</param>
        /// <returns>The concatenation of <paramref name="strings"/> seperated with <paramref name="delimiter"/>.</returns>
        public static string ConcatWith(string[] strings, string delimiter)
        {
            if (strings.Length == 0)
                return string.Empty;

            var totalLength = 0;
            foreach (var s in strings)
                totalLength += s.Length;

            var sb = new StringBuilder(totalLength + (strings.Length * delimiter.Length));
            var str = strings[0];
            sb.Append(str);
            for (var i = 1; i < strings.Length; ++i)
            {
                sb.Append(delimiter);
                sb.Append(strings[i]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Return a left part of <paramref name="inString"/> with length <paramref name="inLength"/>
        /// </summary>
        /// <param name="inString">The full string.</param>
        /// <param name="inLength">Length of the left part to return.</param>
        /// <returns>The left part with a certain length of a string</returns>
        public static string Left(string inString, int inLength)
        {
            if (inLength >= inString.Length)
                return inString;

            if (inLength == 0)
                return string.Empty;

            return inString.Substring(0, inLength);
        }

        /// <summary>
        /// Return a right part of <paramref name="inString"/> with length <paramref name="inLength"/>
        /// </summary>
        /// <param name="inString">The full string.</param>
        /// <param name="inLength">Length of the right part to return.</param>
        /// <returns>The right part with a certain length of a string</returns>
        public static string Right(string inString, int inLength)
        {
            if (inLength == 0)
                return string.Empty;

            if (inLength >= inString.Length)
                inLength = inString.Length;

            return inString.Substring(inString.Length - inLength, inLength);
        }

        /// <summary>
        /// Returns a sub part of a string
        /// </summary>
        /// <param name="inString">The full string.</param>
        /// <param name="inLeftPos">The left pos of the subpart.</param>
        /// <param name="inRightPos">The right pos of the subpart.</param>
        /// <returns>The subpart of the string</returns>
        public static string Mid(string inString, int inLeftPos, int inRightPos)
        {
            inLeftPos = CMath.Clamp(inLeftPos, 0, inString.Length);
            inRightPos = CMath.Clamp(inRightPos, 0, inString.Length);
            if (inLeftPos >= inRightPos)
                return string.Empty;

            return inString.Substring(inLeftPos, inRightPos - inLeftPos);
        }

        /// <summary>
        /// Removes the specified characters from a string and returns a string with those characters removed.
        /// </summary>
        /// <param name="inString">The string to remove the characters from.</param>
        /// <param name="inRemove">The characters to filter.</param>
        /// <returns>A copy of a string without the specified characters</returns>
        public static string Remove(string inString, char[] inRemove)
        {
            var s = inString;
            foreach (var c in inRemove)
            {
                var i = s.IndexOf(c);
                if (i >= 0)
                    s = s.Remove(i, 1);
            }
            return s;
        }

        /// <summary>
        /// Replaces the specified place in <paramref name="inString"/> with <paramref name="inReplacementString"/>.
        /// </summary>
        /// <param name="inString">The original string.</param>
        /// <param name="inStartIndex">Start index of the subpart.</param>
        /// <param name="inLength">Length of the subpart.</param>
        /// <param name="inReplacementString">The replacement string.</param>
        /// <returns>A copy of inString where the subpart <paramref name="inStartIndex"/> to <paramref name="inLength"/>
        /// is replaced with <paramref name="inReplacementString"/></returns>
        public static string Replace(string inString, int inStartIndex, int inLength, string inReplacementString)
        {
            var endIndex = inStartIndex + inLength;
            var str = inString.Substring(0, inStartIndex) + inReplacementString + inString.Substring(endIndex, inString.Length - endIndex);
            return str;
        }

        /// <summary>
        /// Left of the first index where <paramref name="c"/> is found
        /// </summary>
        /// <param name="src">The source string.</param>
        /// <param name="c">Return everything to the left of this character.</param>
        /// <returns>String to the left of c, or the entire string.</returns>
        public static string LeftOf(string src, char c)
        {
            var ret = src;
            var idx = src.IndexOf(c);
            if (idx != -1)
            {
                ret = src.Substring(0, idx);
            }
            return ret;
        }

        /// <summary>
        /// Left of the first index where <paramref name="c"/> is found
        /// </summary>
        /// <param name="src">The source string.</param>
        /// <param name="c">Return everything to the left of this character.</param>
        /// <returns>String to the left of c, or the entire string.</returns>
        public static string LeftOf(string src, string c)
        {
            var ret = src;
            var idx = src.IndexOf(c, StringComparison.Ordinal);
            if (idx != -1)
            {
                ret = src.Substring(0, idx);
            }
            return ret;
        }

        /// <summary>
        /// Left of the nth occurrence where <paramref name="c"/> is found
        /// </summary>
        /// <param name="src">The source string.</param>
        /// <param name="c">Return everything to the left n'th occurrence of this character.</param>
        /// <param name="n">The nth occurrence of <paramref name="c"/>.</param>
        /// <returns>String to the left of c, or the entire string if not found or n is 0.</returns>
        public static string LeftOf(string src, char c, int n)
        {
            var ret = src;
            var idx = -1;
            while (n > 0)
            {
                idx = src.IndexOf(c, idx + 1);
                if (idx == -1)
                {
                    break;
                }
                --n;
            }
            if (idx != -1)
            {
                ret = src.Substring(0, idx);
            }
            return ret;
        }

        /// <summary>
        /// Left of the nth occurrence where <paramref name="c"/> is found
        /// </summary>
        /// <param name="src">The source string.</param>
        /// <param name="c">Return everything to the left n'th occurrence of this string.</param>
        /// <param name="n">The nth occurrence of <paramref name="c"/>.</param>
        /// <returns>String to the left of <paramref name="c"/>, or the entire string if not found or <paramref name="n"/> is 0.</returns>
        public static string LeftOf(string src, string c, int n)
        {
            var ret = src;
            var idx = -1;
            while (n > 0)
            {
                idx = src.IndexOf(c, idx + 1);
                if (idx == -1)
                {
                    break;
                }
                --n;
            }
            if (idx != -1)
            {
                ret = src.Substring(0, idx);
            }
            return ret;
        }

        /// <summary>
        /// Right of the first occurrence of c
        /// </summary>
        /// <param name="src">The source string.</param>
        /// <param name="c">The search char.</param>
        /// <returns>Returns everything to the right of c, or an empty string if c is not found.</returns>
        public static string RightOf(string src, char c)
        {
            var idx = src.IndexOf(c);
            if (idx != -1)
                return src.Substring(idx + 1);
            return string.Empty;
        }

        /// <summary>
        /// Right of the first occurrence of c
        /// </summary>
        /// <param name="src">The source string.</param>
        /// <param name="c">The search char.</param>
        /// <returns>Returns everything to the right of c, or an empty string if c is not found.</returns>
        public static string RightOf(string src, string c)
        {
            var idx = src.IndexOf(c, StringComparison.Ordinal);
            if (idx != -1)
                return src.Substring(idx + 1);
            return string.Empty;
        }

        /// <summary>
        /// Right of the n'th occurrence of c
        /// </summary>
        /// <param name="src">The source string.</param>
        /// <param name="c">The search char.</param>
        /// <param name="n">The occurrence.</param>
        /// <returns>Returns everything to the right of c, or an empty string if c is not found.</returns>
        public static string RightOf(string src, char c, int n)
        {
            var ret = string.Empty;
            var idx = -1;
            while (n > 0)
            {
                idx = src.IndexOf(c, idx + 1);
                if (idx == -1)
                {
                    break;
                }
                --n;
            }

            if (idx != -1)
            {
                ret = src.Substring(idx + 1);
            }

            return ret;
        }

        /// <summary>
        /// Returns everything to the left of the rightmost char c.
        /// </summary>
        /// <param name="src">The source string.</param>
        /// <param name="c">The search char.</param>
        /// <returns>Everything to the left of the rightmost char c, or the entire string.</returns>
        public static string LeftOfRightmostOf(string src, char c)
        {
            var ret = src;
            var idx = src.LastIndexOf(c);
            if (idx != -1)
            {
                ret = src.Substring(0, idx);
            }
            return ret;
        }

        /// <summary>
        /// Returns everything to the right of the rightmost char c.
        /// </summary>
        /// <param name="src">The source string.</param>
        /// <param name="c">The search char.</param>
        /// <returns>Returns everything to the right of the rightmost search char, or an empty string.</returns>
        public static string RightOfRightmostOf(string src, char c)
        {
            var ret = string.Empty;
            var idx = src.LastIndexOf(c);
            if (idx != -1)
            {
                ret = src.Substring(idx + 1);
            }
            return ret;
        }

        /// <summary>
        /// Returns everything between the start and end chars, exclusive.
        /// </summary>
        /// <param name="src">The source string.</param>
        /// <param name="start">The first char to find.</param>
        /// <param name="end">The end char to find.</param>
        /// <returns>The string between the start and stop chars, or an empty string if not found.</returns>
        public static string[] Between(string src, char start, char end)
        {
            var numStartEndOccurences = 0;
            var startEndOpen = 0;
            foreach (var c in src)
            {
                if (c == start)
                {
                    ++startEndOpen;
                }

                if (c == end)
                {
                    --startEndOpen;
                    if (startEndOpen == 0)
                    {
                        numStartEndOccurences++;
                    }
                }
            }
            var ret = new string[numStartEndOccurences];

            numStartEndOccurences = 0;

            var index = 0;
            var startIndex = 0;
            foreach (var c in src)
            {
                if (c == start)
                {
                    if (startEndOpen == 0)
                        startIndex = index + 1;

                    ++startEndOpen;
                }
                if (c == end)
                {
                    --startEndOpen;
                    if (startEndOpen == 0)
                    {
                        ret[numStartEndOccurences] = src.Substring(startIndex, index - startIndex);
                        numStartEndOccurences++;
                    }
                }
                ++index;
            }

            return ret;
        }

        /// <summary>
        /// Returns the number of occurrences of "find".
        /// </summary>
        /// <param name="src">The source string.</param>
        /// <param name="find">The search char.</param>
        /// <returns>The # of times the char occurs in the search string.</returns>
        public static int Count(string src, char find)
        {
            var ret = 0;
            foreach (var s in src)
            {
                if (s == find)
                {
                    ++ret;
                }
            }
            return ret;
        }

        /// <summary>
        /// Returns the rightmost char in src.
        /// </summary>
        /// <param name="src">The source string.</param>
        /// <returns>The rightmost char, or '\0' if the source has zero length.</returns>
        public static char Rightmost(string src)
        {
            var c = '\0';
            if (src.Length > 0)
            {
                c = src[src.Length - 1];
            }
            return c;
        }

        /// <summary>
        /// Remove characters from 'src' starting at 'index' until the 'stop' character is encountered
        /// </summary>
        /// <param name="src">The source string.</param>
        /// <returns>The rightmost char, or '\0' if the source has zero length.</returns>
        public static string RemoveUntil(string src, int index, char stop)
        {
            var i = index;
            while (src[i] != stop) { ++i; }
            src = src.Remove(index, i);
            return src;
        }



        /// <summary>
        /// Scans the string 'text' by using fieldSpecification as the format description
        /// </summary>
        /// <param name="text">The source text.</param>
        /// <param name="format">The format to use for scanning/parsing the source text.</param>
        /// <returns>The values parsed from the text.</returns>
        public static bool Scan(string text, string format, out object[] targets)
        {
            return StringScan.Scan(text, format, out targets);
        }

        /// <summary>
        /// Scans the string 'text' by using fieldSpecification as the format description
        /// </summary>
        /// <param name="text">The source text.</param>
        /// <param name="format">The format to use for scanning/parsing the source text.</param>
        /// <param name="targets">The values parsed from the text, they should match the number of fields specified in 'format'.</param>
        public static bool Scan(string text, string format, params object[] targets)
        {
            return StringScan.Scan(text, format, targets);
        }
    }
}

