using System;
using System.IO;
using Net.Office.Excel.ValueTypes;

namespace Net.Office.Excel.Records
{
	/// <summary>
	/// This class abstracts BOOLERR (0x0205) Excel records found in Excel streams.
	/// </summary>
	public class BoolErrRecord : RowColXfCellRecord
	{
		private BoolErrValue _value;

		/// <summary>
		/// The constructor for the record.
		/// </summary>
		/// <param name="biff">The GenericBiff record that should contain the correct type and data for the BOOLERR record.</param>
		/// <exception cref="InvalidRecordIdException">
		/// An InvalidRecordIdException is thrown if biff contains an invalid type or invalid data.
		/// </exception>
		public BoolErrRecord(GenericBiff biff)
		{
			if(biff.Id == (ushort)RecordType.BoolErr)
			{
				var reader = new BinaryReader(biff.GetDataStream());

				ReadRowColXf(reader);
				
				var boolErr = reader.ReadByte();
				var error = reader.ReadByte();
				_value = new BoolErrValue(boolErr, error);
			}
			else
				throw new InvalidRecordIdException(biff.Id, RecordType.BoolErr);
		}

		/// <summary>
		/// Method for returning the value in the BOOLERR record.
		/// </summary>
		/// <value>
		/// The returned value may be a string or bool. Be sure to test accordingly.
		/// </value>
		public override object Value
		{
			get
			{
				return _value.Value;
			}
		}

	}
}
