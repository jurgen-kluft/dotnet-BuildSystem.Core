using System.Globalization;
using GameCore;

namespace DataBuildSystem
{
    public interface IExcelCharConversion
    {
        char From { get; }
        char To { get; }
    }

    public interface IExcelSheetColumn
    {
        string Name { get; }
        int Column { get; }
        int BeginRow { get; }
        int EndRow { get; }
        byte[] AsciiSet { get; }
    }

    public interface IExcelSheetConfig
    {
        int BeginRow { get; }
        int EndRow { get; }
        IExcelSheetColumn[] Columns { get; }
    }

    public interface ILocalizerConfig
    {
        /// <summary>
        /// The platform this configuration is for
        /// </summary>
        EPlatform Platform { get; }

        string SubDepFileExtension { get; }
        string MainDepFileExtension { get; }
        string SubLocFileExtension { get; }
        string MainLocFileExtension { get; }
    }

}
