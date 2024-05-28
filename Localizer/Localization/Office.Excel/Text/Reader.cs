using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Net.SourceForge.Koogra.Text
{
	/// <summary>
	/// Utility class for reading Text from BinaryReaders
	/// </summary>
	public class Reader
	{
		private static Encoding _asciiEncoder = new ASCIIEncoding();
		private static Encoding _unicodeEncoder = new UnicodeEncoding();

		/// <summary>
		/// Reads a string where the first 2 bytes are its length and is followed by unicode characters.
		/// </summary>
		/// <param name="reader">The string reader.</param>
		/// <returns>Returns a string.</returns>
		public static string ReadSimpleUnicodeString(BinaryReader reader)
		{
			var len = reader.ReadUInt16();
			return ReadSimpleUnicodeString(reader, len);
		}

		/// <summary>
		/// Reads an ascii string given its length.
		/// </summary>
		/// <param name="reader">The string reader.</param>
		/// <param name="len">The length of the string.</param>
		/// <returns>Returns a string.</returns>
		public static string ReadSimpleAsciiString(BinaryReader reader, int len)
		{
			return _asciiEncoder.GetString(reader.ReadBytes(len));
		}

		/// <summary>
		/// Reads a unicode string given its length.
		/// </summary>
		/// <param name="reader">The string reader.</param>
		/// <param name="len">The length of the string.</param>
		/// <returns>Returns a string.</returns>
		public static string ReadSimpleUnicodeString(BinaryReader reader, int len)
		{
			return _unicodeEncoder.GetString(reader.ReadBytes(len));
		}

		/// <summary>
		/// Reads an encoded string where the length is followed by options that determine if the string is ascii, unicode, rtf, etc.
		/// </summary>
		/// <param name="reader">The string reader.</param>
		/// <returns>Returns a string.</returns>
		public static string ReadComplexString(BinaryReader reader)
		{
			var len = reader.ReadUInt16();
			return ReadComplexString(reader, len);
		}

		/// <summary>
		/// Reads an encoded string given its length where the first byte are options that determine if the string is ascii, unicode, rtf, etc.
		/// </summary>
		/// <param name="reader">The string reader.</param>
		/// <param name="len">The string length.</param>
		/// <returns>Returns a string.</returns>
		public static string ReadComplexString(BinaryReader reader, int len)
		{
			var options = reader.ReadByte();
			var compressed = (options & 0x01) == 0;

			Encoding enc;
			if(compressed)
				enc = _asciiEncoder;
			else
			{
				len *= 2;
				enc = _unicodeEncoder;
			}

			var retVal = enc.GetString(reader.ReadBytes(len));

			return retVal;
		}
	}
}
