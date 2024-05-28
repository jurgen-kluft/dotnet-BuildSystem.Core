using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Net.Office.Excel.Records
{
	/// <summary>
	/// This class abstracts the SST (0x00fc) Excel record found in Excel streams.
	/// </summary>
	public class SstRecord : Biff
	{
		private string[] _strings;

		private ContinueRecord ReadContinue(Stream stream)
		{
			var biff = new GenericBiff(stream);
			return new ContinueRecord(biff);
		}

		/// <summary>
		/// The constructor for the record.
		/// </summary>
		/// <param name="biff">The GenericBiff record that should contain the correct type and data for the SST record.</param>
		/// <param name="recordStream">The stream into the records to which the SST record belongs to. The record stream must be positioned just after the SST record.</param>
		/// <exception cref="InvalidRecordIdException">
		/// An InvalidRecordIdException is thrown if biff contains an invalid type or invalid data.
		/// </exception>
		public SstRecord(GenericBiff biff, Stream recordStream)
		{
			if(biff.Id == (ushort)RecordType.Sst)
			{
				var stream = biff.GetDataStream();
				var reader = new BinaryReader(stream);
				var totalStrings = reader.ReadUInt32();
				var totalUniqueStrings = reader.ReadUInt32();

				_strings = new string[totalUniqueStrings];
				var sb = new StringBuilder();
				for(var i = 0; i < totalUniqueStrings; ++i)
				{
					if(stream.Position >= stream.Length)
					{
						var cont = ReadContinue(recordStream);
						stream = cont.GetDataStream();
						reader = new BinaryReader(stream);
					}

					var len = reader.ReadUInt16();
					var options = reader.ReadByte();
					var compressed = (options & 0x01) == 0;
					var farEast = (options & 0x04) != 0;
					var richText = (options & 0x08) != 0;
					ushort rtSize = 0;
					uint farEastSize = 0;

					if(richText)
						rtSize = reader.ReadUInt16();
					if(farEast)
						farEastSize = reader.ReadUInt32();
		
					sb.Length = 0;
					sb.EnsureCapacity(len);
					for(ushort n = 0; n < len; ++n)
					{
						if(stream.Position >= stream.Length)
						{
							var cont = ReadContinue(recordStream);
							stream = cont.GetDataStream();
							reader = new BinaryReader(stream);
							compressed = (reader.ReadByte() & 0x01) == 0;
						}

						if(compressed)
							sb.Append(Convert.ToChar(reader.ReadByte()));
						else
							sb.Append(Convert.ToChar(reader.ReadUInt16()));
					}
					Debug.Assert(sb.Length == len);
					_strings[i] = sb.ToString();

					var skip = (rtSize * 4) + farEastSize;

					while(skip > 0)
					{
						if(stream.Position >= stream.Length)
						{
							var cont = ReadContinue(recordStream);
							stream = cont.GetDataStream();
							reader = new BinaryReader(stream);
						}

						var actualSkip = Math.Min(stream.Length - stream.Position, skip);

						stream.Seek(actualSkip, SeekOrigin.Current);

						skip -= actualSkip;
					}					
				}
			}
			else
				throw new InvalidRecordIdException(biff.Id, RecordType.Sst);
		}

		/// <summary>
		/// The array of strings in the SST record.
		/// </summary>
		public string[] Strings
		{
			get
			{
				return _strings;
			}
		}
	}
}
