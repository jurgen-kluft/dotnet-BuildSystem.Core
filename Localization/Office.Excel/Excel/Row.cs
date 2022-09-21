using System;
using System.Collections.Generic;
using System.Diagnostics;
using Net.Office.Excel.Records;

namespace Net.Office.Excel
{
	/// <summary>
	/// Represents a row of cells.
	/// </summary>
	public class Row : ExcelObject
	{
		private CellCollection _cells;

		internal Row(Workbook wb, RowRecord row) : base(wb)
		{
			_cells = new CellCollection(wb);
		}

		/// <summary>
		/// The collection of cells in the row.
		/// </summary>
		public CellCollection Cells
		{
			get
			{
				return _cells;
			}
		}

	}
}
