using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace BigfilePacker
{
    /// <summary>
    /// Arguments class
    /// </summary>
    public class Arguments
    {
        #region Fields

        // Variables
        private StringDictionary mParameters;

        #endregion
        #region Public Methods

        public bool HasParameter(string inParamStr)
        {
            return (mParameters.ContainsKey(inParamStr));
        }

        public bool AddParameter(string inParamStr)
        {
            if (!mParameters.ContainsKey(inParamStr))
            {
                mParameters.Add(inParamStr, "true");
                return true;
            }
            return false;
        }
        public bool AddParameter(string inParamStr, string inValueStr)
        {
            if (!mParameters.ContainsKey(inParamStr))
            {
                mParameters.Add(inParamStr, inValueStr);
                return true;
            }
            return false;
        }

        #endregion
        #region Constructor

        // Constructor
        public Arguments(string[] Args)
        {
            mParameters = new StringDictionary();
            Regex Spliter = new Regex(@"^-{1,2}|^/|=", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Regex Remover = new Regex(@"^['""]?(.*?)['""]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            string parameterStr = null;
            string[] Parts;

            // Valid parameters forms:
            // {-,/,--}param{ ,=,:}((",')value(",'))
            // Examples: -param1 value1 --param2 /param3:"Test-:-work" /param4=happy -param5 '--=nice=--'
            foreach (string Txt in Args)
            {
                // Look for new parameters (-,/ or --) and a possible enclosed value (=,:)
                Parts = Spliter.Split(Txt, 3);
                switch (Parts.Length)
                {
                    // Found a value (for the last parameter found (space separator))
                    case 1:
                        if (parameterStr != null)
                        {
                            if (!mParameters.ContainsKey(parameterStr))
                            {
                                Parts[0] = Remover.Replace(Parts[0], "$1");
                                mParameters.Add(parameterStr, Parts[0]);
                            }
                            parameterStr = null;
                        }
                        // else Error: no parameter waiting for a value (skipped)
                        break;
                    // Found just a parameter
                    case 2:
                        // The last parameter is still waiting. With no value, set it to true.
                        if (parameterStr != null)
                        {
                            if (!mParameters.ContainsKey(parameterStr)) mParameters.Add(parameterStr, "true");
                        }
                        parameterStr = Parts[1];
                        break;
                    // Parameter with enclosed value
                    case 3:
                        // The last parameter is still waiting. With no value, set it to true.
                        if (parameterStr != null)
                        {
                            if (!mParameters.ContainsKey(parameterStr)) mParameters.Add(parameterStr, "true");
                        }
                        parameterStr = Parts[1];
                        // Remove possible enclosing characters (",')
                        if (!mParameters.ContainsKey(parameterStr))
                        {
                            Parts[2] = Remover.Replace(Parts[2], "$1");
                            mParameters.Add(parameterStr, Parts[2]);
                        }
                        parameterStr = null;
                        break;
                }
            }
            // In case a parameter is still waiting
            if (parameterStr != null)
            {
                if (!mParameters.ContainsKey(parameterStr)) mParameters.Add(parameterStr, "true");
            }
        }

        #endregion
        #region Properties

        public bool IsEmpty
        {
            get
            {
                return (mParameters.Count == 0);
            }
        }

        // Retrieve a parameter value if it exists
        public string this[string Param]
        {
            get
            {
                return (mParameters[Param]);
            }
        }

        #endregion
    }
}
