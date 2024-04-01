using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GameData;

namespace DataBuildSystem
{
	internal sealed class GameDataBigfile
	{
        /// <summary>
        /// A file to add to the data archive
        /// </summary>
        private static void Add(Int64 fileId, string[] filenames, Bigfile bigfile, List<BigfileFile> children)
        {
            BigfileFile mainBigfileFile = new(filenames[0]);
            for (var i = 1; i < filenames.Length; ++i)
            {
                var filename = filenames[i];
                BigfileFile bigfileFile = new(filename);
                bigfileFile.FileId = -1;
                mainBigfileFile.Children.Add(bigfileFile);
                children.Add(bigfileFile);
            }

            // TODO Determine the Bigfile linked to this FileId
            bigfile.Files.Add(mainBigfileFile);
        }

        public void Save(string filename, List<DataCompilerOutput> gdClOutput)
		{
			var bfb = new BigfileBuilder(BigfileConfig.Platform);

            Bigfile bigfile = new();
            List<BigfileFile> children = new();
            Int64 fileId = 0;
            foreach (var o in gdClOutput)
			{
				Add(fileId++, o.Filenames, bigfile, children);
			}
            foreach(var c in children)
			{
                c.FileId = fileId++;
                bigfile.Files.Add(c);
			}

            List<Bigfile> bigFiles = new() { bigfile };
            bfb.Save(BuildSystemCompilerConfig.PubPath, BuildSystemCompilerConfig.DstPath, filename, bigFiles);
		}
	}
}
