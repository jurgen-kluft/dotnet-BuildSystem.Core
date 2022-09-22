using System.Text;

namespace GameCore
{
    /// <summary>
    /// A helper class to build a command line
    /// </summary>
    /// <remarks>
    ///   Surrounds the value between quotes if it contains spaces
    ///   Separates "arg,value" with a space
    ///   Can append a value with a format
    ///   Can append conditionally
    /// </remarks>
    public class CommandLineBuilder
    {
        private StringBuilder builder = new StringBuilder();

        public void Clear()
        {
            builder.Remove(0, builder.Length);
        }

        public void AppendArgument(string format, string value)
        {
            if (StringTools.IsBlank(value))
                return;

            AppendSpaceIfNotEmpty();
            builder.AppendFormat(format, value);
        }

        public void AppendArgument(string value)
        {
            if (StringTools.IsBlank(value))
                return;

            AppendSpaceIfNotEmpty();
            builder.Append(value);
        }

        private void AppendSpaceIfNotEmpty()
        {
            if (builder.Length > 0) 
                builder.Append(" ");
        }

        public void AppendIf(bool condition, string value)
        {
            if (condition) 
                AppendArgument(value);
        }

        public void AppendIf(bool condition, string format, string argument)
        {
            if (condition) 
                AppendArgument(format, argument);
        }

        public void Append(string text)
        {
            builder.Append(text);
        }

        public void AddArgument(string arg, string value)
        {
            AddArgument(arg, " ", value);
        }

        public void AddArgument(string arg, string separator, string value)
        {
            if (StringTools.IsBlank(value))
                return;

            AppendSpaceIfNotEmpty();

            builder.Append(string.Format("{0}{1}{2}", arg, separator, StringTools.SurroundInQuotesIfContainsSpace(value)));
        }

        public void AddArgument(string value)
        {
            if (StringTools.IsBlank(value)) 
                return;

            AppendSpaceIfNotEmpty();

            builder.Append(StringTools.SurroundInQuotesIfContainsSpace(value));
        }

        public override string ToString()
        {
            return builder.ToString();
        }
    }
}