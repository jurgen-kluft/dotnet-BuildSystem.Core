using System;
using System.Globalization;

namespace DataBuildSystem
{
    public interface IDependencySystemConfig
    {
        #region Methods & Properties
        
        /// <summary>
        /// The extension of the dependency files (.dep)
        /// </summary>
        string Extension { get; }

        /// <summary>
        /// True = If the DateTime of a file has changed then regard the file as changed
        /// </summary>
        bool DateTimeComparison { get; }

        /// <summary>
        /// True = If the MD5 hash of the binary content has changed then regard the file as changed
        /// </summary>
        bool BinaryComparison { get; }

        /// <summary>
        /// True = Filter rejected folder
        /// </summary>
        bool FolderFilter(string folder);

        /// <summary>
        /// True = Filter rejected file
        /// </summary>
        bool FileFilter(string folder);

        #endregion
    }

    public class DependencySystemDefaultConfig : IDependencySystemConfig
    {
        #region Methods & Properties

        public string Extension { get { return ".dep"; } }
        public bool DateTimeComparison { get { return true; } }
        public bool BinaryComparison { get  { return false; } }
        public bool FolderFilter(string folder) { return false; } 
        public bool FileFilter(string folder) { return false; } 

        #endregion
    }

    public static class DependencySystemConfig
    {
        #region Fields
        
        private static IDependencySystemConfig sConfig = new DependencySystemDefaultConfig();

        #endregion
        #region Init

        public static void Init(IDependencySystemConfig config)
        {
            sConfig = config;
        }

        #endregion
        #region Methods & Properties

        public static string Extension { get { return sConfig.Extension; } }
        public static bool DateTimeComparison { get { return sConfig.DateTimeComparison; } }
        public static bool BinaryComparison { get { return sConfig.BinaryComparison; } }
        public static bool FolderFilter(string folder) { return sConfig.FolderFilter(folder); }
        public static bool FileFilter(string folder) { return sConfig.FileFilter(folder); }

        #endregion
    }
}
