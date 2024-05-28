using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Net.SourceForge.Koogra.Storage.Sectors;

namespace Net.SourceForge.Koogra.Storage
{
	/// <summary>
	/// CompoundFile.
	/// </summary>
	public class CompoundFile
	{
		private Directory _directory;

		/// <summary>
		/// Stream constructor.
		/// </summary>
		/// <param name="stream">The stream that contains the CompoundFile.</param>
		public CompoundFile(Stream stream)
		{
			Init(stream);
		}

		private void Init(Stream stream)
		{
			// stream size sanity checker
			// compound files are always blocks of 512 bytes
			Debug.Assert((stream.Length % 512) == 0);
			
			// read in the first sector
			var sector = new StorageSector(stream);
			// interpret sector as header sector
			var header = new HeaderSector(sector.GetStream());

			// read in all remaining sectors
			var sectors = new SectorCollection((int)(stream.Length / Constants.SECTOR_SIZE));
			while(stream.Position != stream.Length)
			{
				sector = new StorageSector(stream);
				sectors.Add(sector);
			}

			// build the fat index
			var index = new List<Sect>((int)(Constants.MAX_SECT * header.SectFatCount));

			// read first 109 fat entries
			for(var i = 0; i < header.SectFat.Length; ++i)
			{
				var fatSect = header.SectFat[i];
				if(!fatSect.IsFree)
				{
					var fat = new FatSector(((StorageSector)sectors[fatSect]).GetStream());
					index.AddRange(fat.SectFat);
					sectors[fatSect] = fat;
				}
			}

			// read remaining fat entries
			int difCount;
			Sect difIndex;									 
			for(difIndex = header.SectDifStart, difCount = 0;
				!difIndex.IsEndOfChain && difCount < header.SectDifCount; 
				++difCount)
			{
				var dif = new DifSector(((StorageSector)sectors[difIndex]).GetStream());
				sectors[difIndex] = dif;

				for(var i = 0; i < dif.SectFat.Length; ++i)
				{
					var fatSect = dif.SectFat[i];

					if(!fatSect.IsFree)
					{
						var fat = new FatSector(((StorageSector)sectors[fatSect]).GetStream());
						index.AddRange(fat.SectFat);
						sectors[fatSect] = fat;
					}
				}
				
				difIndex = dif.NextDif;
			}
			Debug.Assert(difCount == header.SectDifCount);

			// read in mini fat sectors
			Debug.Assert(index.Count == (header.SectFatCount * Constants.MAX_SECT));
			Debug.Assert(index.Capacity == index.Count);

			var fatSects = index.ToArray();

			Sect miniFatSect;
			int miniFatCount;
			for(miniFatSect = header.SectMiniFatStart, miniFatCount = 0;
				!miniFatSect.IsEndOfChain && miniFatCount < header.SectMiniFatCount;
				miniFatSect = fatSects[miniFatSect.ToInt()], ++miniFatCount)
			{
				var miniFat = new MiniFatSector(((StorageSector)sectors[miniFatSect]).GetStream());
				sectors[miniFatSect] = miniFat;
			}

			Debug.Assert(miniFatCount == header.SectMiniFatCount);

			// read in directory sectors
			var dirs = new DirectorySectorEntryCollection();

			for(var dirSect = header.SectDirStart;
				!dirSect.IsEndOfChain;
				dirSect = fatSects[dirSect.ToInt()])
			{
				var dir = new DirectorySector(((StorageSector)sectors[dirSect]).GetStream());
				
				foreach(var entry in dir.Entries)
					dirs.Add(entry);

				sectors[dirSect] = dir;
			}

			_directory = new Directory(dirs, sectors, fatSects);
		}

		/// <summary>
		/// Opens a stream by locating the stream by name.
		/// </summary>
		/// <param name="name">The name of the string.</param>
		/// <returns>Returns the stream.</returns>
		/// <exception cref="IOException">Exception is thrown if the stream does not exist.</exception>
		public Stream OpenStream(string name)
		{
			var entry = _directory.Root.Find(name);
			if(entry != null && entry is StreamEntry)
				return new MemoryStream(((StreamEntry)entry).Data);

			throw new IOException("Stream [" + name + "] was not found.");
		}
	}
}
