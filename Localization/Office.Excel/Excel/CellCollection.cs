using System;
using System.Collections.Generic;
using System.Diagnostics;
using Net.SourceForge.Koogra.Collections;

namespace Net.Office.Excel
{
	/// <summary>
	/// Collection class for Cell objects.
	/// </summary>
	public class CellCollection : ExcelIndexedCollection<byte, Cell>
	{
		internal CellCollection(Workbook wb) : base(wb)
		{
		}

		internal void Add(byte index, Cell cell)
		{
			Debug.Assert(!ContainsKey(index));
			BaseAdd(index, cell);
		}

		/// <summary>
		/// The first column in the collection.
		/// </summary>
		public byte FirstCol
		{
			get
			{
				return BaseFirstKey;
			}
		}

		/// <summary>
		/// The last column in the collection.
		/// </summary>
		public byte LastCol
		{
			get
			{
				return BaseLastKey;
			}
		}

		/// <summary>
		/// The indexer for the collection.
		/// </summary>
		public Cell this[byte index]
		{
			get
			{                
				return BaseGet(index);
			}
		}

		
	}
}
