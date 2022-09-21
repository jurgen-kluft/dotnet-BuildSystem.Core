using System;
using System.Collections.Generic;
using Net.Office.Excel.Records;

namespace Net.Office.Excel
{
	/// <summary>
	/// Represents the table of hyperlinks to be found in a worksheet
	/// </summary>
	public class HyperLinkCollection : ExcelCollection<HyperLinkRecord>
	{
		internal HyperLinkCollection(Workbook wb) : base(wb)
		{
		}

		/// <summary>
		/// Method for locating a hyperlink given a row or column.
		/// </summary>
		/// <param name="row">The row to search in.</param>
		/// <param name="col">The column to search in.</param>
		/// <returns>Returns a HyperLinkRecord if a record is found, null if not.</returns>
		public HyperLinkRecord FindHyperlink(ushort row, ushort col)
		{
			foreach(HyperLinkRecord h in this)
			{
				if(h.FirstRow >= row && h.LastRow <= row &&
					h.FirstCol >= col && h.LastCol <= col)
				{
					return h;
				}
			}

			return null;
		}
	}
}