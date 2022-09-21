using System;
using System.Collections.Generic;
using System.Diagnostics;
using Net.SourceForge.Koogra.Collections;

namespace Net.Office.Excel
{
	/// <summary>
	/// Collection class for Row objects.
	/// </summary>
	public class RowCollection : ExcelIndexedCollection<ushort, Net.Office.Excel.Row>
	{
		internal RowCollection(Workbook wb) : base(wb)
		{
		}

		internal void Add(ushort rowNumber, Net.Office.Excel.Row row)
		{
			Debug.Assert(!ContainsKey(rowNumber));
			BaseAdd(rowNumber, row);
		}

		/// <summary>
		/// The first row in the collection.
		/// </summary>
		public ushort FirstRow
		{
			get
			{
				return BaseFirstKey;
			}
		}

		/// <summary>
		/// The last row in the collection.
		/// </summary>
		public ushort LastRow
		{
			get
			{
				return BaseLastKey;
			}
		}

		/// <summary>
		/// The indexer for the collection.
		/// </summary>
		public Row this[ushort index]
		{
			get
			{
				return BaseGet(index);
			}
		}
	}
}
