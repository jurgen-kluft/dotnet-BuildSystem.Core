using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;

namespace GameCore
{
	/// <summary>
	/// Summary description for Scanner.
	/// </summary>
	public class StringScan
	{
		protected static readonly Hashtable typePatterns;
        static StringScan()
		{
			typePatterns = new Hashtable();

			typePatterns.Add("String",@"[\w\d\S]+");
			typePatterns.Add("Int16",  @"-[0-9]+|[0-9]+");
			typePatterns.Add("UInt16",  @"[0-9]+");
			typePatterns.Add("Int32",  @"-[0-9]+|[0-9]+");
			typePatterns.Add("UInt32",  @"[0-9]+");
			typePatterns.Add("Int64",   @"-[0-9]+|[0-9]+");
			typePatterns.Add("UInt64",   @"[0-9]+");
			typePatterns.Add("Single",   @"[-]|[.]|[-.]|[0-9][0-9]*[.]*[0-9]+");
			typePatterns.Add("Double",   @"[-]|[.]|[-.]|[0-9][0-9]*[.]*[0-9]+");
			typePatterns.Add("Boolean",   @"true|false");
			typePatterns.Add("Byte",  @"[0-9]{1,3}");
			typePatterns.Add("SByte",  @"-[0-9]{1,3}|[0-9]{1,3}");
			typePatterns.Add("Char",  @"[\w\S]{1}");
			typePatterns.Add("Decimal", @"[-]|[.]|[-.]|[0-9][0-9]*[.]*[0-9]+");
		}

		/// <summary>
		/// Scan mimics scanf.
        /// 
		/// A master regular expression pattern is created that will group each "word" in the text and using regex grouping
		/// extract the values for the field specifications.
		/// Example text: "Hello true 6.5"  fieldSpecification: "{String} {Boolean} {Double}"
		/// The fieldSpecification will result in the generation of a master Pattern:
		/// ([\w\d\S]+)\s+(true|false)\s+([-]|[.]|[-.]|[0-9][0-9]*[.]*[0-9]+)
		/// This masterPattern is ran against the text string and the groups are extracted.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="fieldSpecification">A string that may contain simple field specifications of the form {Int16}, {String}, etc</param>
		/// <returns>object[] that contains values for each field</returns>
		public static bool Scan(string text, string fieldSpecification, out object[] targets)
		{
			targets = null;
			try
			{
				var targetMatchGroups = new ArrayList();
				var targetTypes = new ArrayList();

				var matchingPattern = "";
				Regex reggie = null;
				MatchCollection matches = null;

				//masterPattern is going to hold a "big" regex pattern that will be ran against the original text
				var masterPattern = fieldSpecification.Trim();
				matchingPattern =  @"(\S+)";
				masterPattern = Regex.Replace(masterPattern,matchingPattern,"($1)");		//insert grouping parens

				//store the group location of the format tags so that we can select the correct group values later.
				matchingPattern = @"(\([\w\d\S]+\))";
				reggie = new Regex(matchingPattern);
				matches = reggie.Matches(masterPattern);
				for(var i = 0; i < matches.Count; i++)
				{
					var m = matches[i];
					var sVal = m.Groups[1].Captures[0].Value;

					//is this value a {n} value. We will determine this by checking for {
					if(sVal.IndexOf('{') >= 0)
					{
						targetMatchGroups.Add(i);
						var p = @"\(\{(\w*)\}\)";	//pull out the type
						sVal = Regex.Replace(sVal,p,"$1");
						targetTypes.Add(sVal);
					}
				}
			
				//Replace all of the types with the pattern that matches that type
				masterPattern = Regex.Replace(masterPattern,@"\{String\}",  (string)typePatterns["String"]);
				masterPattern = Regex.Replace(masterPattern,@"\{Int16\}",  (string)typePatterns["Int16"]);
				masterPattern = Regex.Replace(masterPattern,@"\{UInt16\}",  (string)typePatterns["UInt16"]);
				masterPattern = Regex.Replace(masterPattern,@"\{Int32\}",  (string)typePatterns["Int32"]);
				masterPattern = Regex.Replace(masterPattern,@"\{UInt32\}",  (string)typePatterns["UInt32"]);
				masterPattern = Regex.Replace(masterPattern,@"\{Int64\}",  (string)typePatterns["Int64"]);
				masterPattern = Regex.Replace(masterPattern,@"\{UInt64\}",   (string)typePatterns["UInt64"]);
				masterPattern = Regex.Replace(masterPattern,@"\{Single\}",   (string)typePatterns["Single"]);
				masterPattern = Regex.Replace(masterPattern,@"\{Double\}",   (string)typePatterns["Double"]);
				masterPattern = Regex.Replace(masterPattern,@"\{Boolean\}",   (string)typePatterns["Boolean"]);
				masterPattern = Regex.Replace(masterPattern,@"\{Byte\}",  (string)typePatterns["Byte"]);
				masterPattern = Regex.Replace(masterPattern,@"\{SByte\}",  (string)typePatterns["SByte"]);
				masterPattern = Regex.Replace(masterPattern,@"\{Char\}",  (string)typePatterns["Char"]);
				masterPattern = Regex.Replace(masterPattern,@"\{Decimal\}", (string)typePatterns["Decimal"]);
				
				masterPattern = Regex.Replace(masterPattern,@"\s+","\\s+");	//replace the white space with the pattern for white space

				//run our generated pattern against the original text.
				reggie = new Regex(masterPattern);
				matches = reggie.Matches(text);
				//PrintMatches(matches);

				//allocate the targets
                if (targetMatchGroups.Count > 0)
                {
                    targets = new object[targetMatchGroups.Count];
                    for (var x = 0; x < targetMatchGroups.Count; x++)
                    {
                        var i = (int)targetMatchGroups[x];
                        var tName = (string)targetTypes[x];
                        if (i < matches[0].Groups.Count)
                        {
                            //add 1 to i because i is a result of serveral matches each resulting in one group.
                            //this query is one match resulting in serveral groups.
                            var sValue = matches[0].Groups[i + 1].Captures[0].Value;
                            targets[x] = ReturnValue(tName, sValue);
                        }
                    }
                }
			}
			catch(Exception)
			{
                return false;
			}

            return targets != null;
		}//Scan

		/// Scan mimics scanf.
        /// 
		/// A master regular expression pattern is created that will group each "word" in the text and using regex grouping
		/// extract the values for the field specifications.
		/// 
        /// Example text: "Hello true 6.5"  fieldSpecification: "{0} {1} {2}" and the target array has objects of these types: "String, ,Boolean, Double"
		/// The targets are scanned and each target type is extracted in order to build a master pattern based on these types
		/// The fieldSpecification and target types will result in the generation of a master Pattern:
		/// ([\w\d\S]+)\s+(true|false)\s+([-]|[.]|[-.]|[0-9][0-9]*[.]*[0-9]+)
        /// 
		/// This masterPattern is ran against the text string and the groups are extracted and placed back into the targets
        /// 
		/// <param name="text"></param>
		/// <param name="fieldSpecification"></param>
		/// <param name="targets"></param>
        public static bool Scan(string text, string fieldSpecification, params object[] targets)
		{
			try
			{
				var targetMatchGroups = new ArrayList();

				var matchingPattern = "";
				Regex reggie = null;
				MatchCollection matches = null;

				//masterPattern is going to hold a "big" regex pattern that will be ran against the original text
				var masterPattern = fieldSpecification.Trim();
				matchingPattern =  @"(\S+)";
				masterPattern = Regex.Replace(masterPattern,matchingPattern,"($1)");		//insert grouping parens

				//store the group location of the format tags so that we can select the correct group values later.
				matchingPattern = @"(\([\w\d\S]+\))";
				reggie = new Regex(matchingPattern);
				matches = reggie.Matches(masterPattern);
				for(var i = 0; i < matches.Count; i++)
				{
					var m = matches[i];
					var sVal = m.Groups[1].Captures[0].Value;

					//is this value a {n} value. We will determine this by checking for {
					if(sVal.IndexOf('{') >= 0)
					{
						targetMatchGroups.Add(i);
					}
				}
			
				matchingPattern = @"(\{\S+\})";	//match each paramter tag of the format {n} where n is a digit
				reggie = new Regex(matchingPattern);
				matches = reggie.Matches(masterPattern);

				for(var i = 0; i < targets.Length && i < matches.Count; i++)
				{
					//string groupID = String.Format("${0}",(i+1));
					var innerPattern = "";

					var t = targets[i].GetType();
					innerPattern = ReturnPattern(t.Name);

					//replace the {n} with the type's pattern
					var groupPattern = "\\{" + i + "\\}";
					masterPattern = Regex.Replace(masterPattern,groupPattern,innerPattern);
				}

				masterPattern = Regex.Replace(masterPattern,@"\s+","\\s+");	//replace white space with the whitespace pattern

				//run our generated pattern against the original text.
				reggie = new Regex(masterPattern);
				matches = reggie.Matches(text);
                if (matches.Count > 0)
                {
                    for (var x = 0; x < targetMatchGroups.Count; x++)
                    {
                        var i = (int)targetMatchGroups[x];
                        if (i < matches[0].Groups.Count)
                        {
                            //add 1 to i because i is a result of serveral matches each resulting in one group.
                            //this query is one match resulting in serveral groups.
                            var sValue = matches[0].Groups[i + 1].Captures[0].Value;
                            var t = targets[x].GetType();
                            targets[x] = ReturnValue(t.Name, sValue);
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
			}
			catch(Exception)
			{
                return false;
            }
		}	//Scan

		/// <summary>
		/// Return the Value inside of an object that boxes the built in type or references the string
		/// </summary>
		/// <param name="typeName"></param>
		/// <param name="sValue"></param>
		/// <returns></returns>
        private static object ReturnValue(string typeName, string sValue)
		{
			object o = null;
			switch(typeName)
			{
				case "String":
					o = sValue;
					break;
														
				case "Int16":
					o = short.Parse(sValue);
					break;

				case "UInt16":
					o = ushort.Parse(sValue);
					break;

				case "Int32":
					o = int.Parse(sValue);
					break;

				case "UInt32":
					o = uint.Parse(sValue);
					break;

				case "Int64":
					o = long.Parse(sValue);
					break;

				case "UInt64":
					o = ulong.Parse(sValue);
					break;

				case "Single":
					o = float.Parse(sValue);
					break;

				case "Double":
					o = double.Parse(sValue);
					break;

				case "Boolean":
					o = bool.Parse(sValue);
					break;

				case "Byte":
					o = byte.Parse(sValue);
					break;

				case "SByte":
					o = sbyte.Parse(sValue);
					break;

				case "Char":
					o = char.Parse(sValue);
					break;

				case "Decimal":
					o = decimal.Parse(sValue);
					break;
			}
			return o;
		}//ReturnValue

		/// <summary>
		/// Return a pattern for regular expressions that will match the built in type specified by name
		/// </summary>
		/// <param name="typeName"></param>
		/// <returns></returns>
        private static string ReturnPattern(string typeName)
		{
			var innerPattern = "";
			switch(typeName)
			{
				case "Int16":
					innerPattern = (string)typePatterns["Int16"];
					break;

				case "UInt16":
					innerPattern = (string)typePatterns["UInt16"];
					break;

				case "Int32":
					innerPattern = (string)typePatterns["Int32"];
					break;

				case "UInt32":
					innerPattern = (string)typePatterns["UInt32"];
					break;

				case "Int64":
					innerPattern = (string)typePatterns["Int64"];
					break;

				case "UInt64":
					innerPattern = (string)typePatterns["UInt64"];
					break;

				case "Single":
					innerPattern = (string)typePatterns["Single"];
					break;

				case "Double":
					innerPattern = (string)typePatterns["Double"];
					break;

				case "Boolean":
					innerPattern = (string)typePatterns["Boolean"];
					break;

				case "Byte":
					innerPattern = (string)typePatterns["Byte"];
					break;

				case "SByte":
					innerPattern = (string)typePatterns["SByte"];
					break;

				case "Char":
					innerPattern = (string)typePatterns["Char"];
					break;

				case "Decimal":
					innerPattern = (string)typePatterns["Decimal"];
					break;

				case "String":
					innerPattern = (string)typePatterns["String"];
					break;
			}
			return innerPattern;
		}	//ReturnPattern

		private static void PrintMatches(MatchCollection matches)
		{
			Console.WriteLine("===---===---===---===");
			var matchCount = 0;
			Console.WriteLine("Match Count = " + matches.Count);
			foreach(Match m in matches)
			{
				if(m == Match.Empty) Console.WriteLine("Empty match");
                Console.WriteLine("Match" + (++matchCount));
				for (var i = 0; i < m.Groups.Count; i++) 
				{
					var g = m.Groups[i];
                    Console.WriteLine("Group" + i + "='" + g + "'");
					var cc = g.Captures;
					for (var j = 0; j < cc.Count; j++) 
					{
						var c = cc[j];
						Console.Write("Capture"+j+"='" + c + "', Position="+c.Index + "   <");
						for(var k = 0; k < c.ToString().Length; k++)
						{
                            Console.Write(((int)(c.ToString()[k])));
						}
                        Console.WriteLine(">");
					}
				}
			}
		}
	}

	/// <summary>
    /// Exceptions that are thrown by this namespace and the StringScan Class
	/// </summary>
	class ScanException : Exception
	{
		public ScanException() : base()
		{
		}

		public ScanException(string message) : base(message)
		{
		}

		public ScanException(string message, Exception inner) : base(message, inner)
		{
		}

		public ScanException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
