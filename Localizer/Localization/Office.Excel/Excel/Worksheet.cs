using System;
using System.Collections.Generic;
using System.Diagnostics;
using Net.Office.Excel.Records;

namespace Net.Office.Excel
{
	/// <summary>
	/// Represents a worksheet in a workbook.
	/// </summary>
	public class Worksheet : ExcelObject
	{
		private string _name;
		private RowCollection _rows;
		private HyperLinkCollection _hyperlinks;

		internal Worksheet(Workbook wb, BoundSheetRecord sheet, SortedList<long, Biff> records) : base(wb)
		{
			_name = sheet.Name;

			var idx = records.IndexOfKey((long)sheet.BofPos);

			_hyperlinks = new HyperLinkCollection(wb);

			for(var i = idx + 1; i < records.Count; ++i)
			{
				var biff = records.Values[i];
				if(biff is HyperLinkRecord)
					_hyperlinks.Add((HyperLinkRecord)biff);
				else if(biff is EofRecord)
					break;
			}
			
			var bof = (BofRecord)records.Values[idx++];

			var seeker = records.Values[idx++];

			while(!(seeker is IndexRecord))
				seeker = records.Values[idx++];

			var index = (IndexRecord)seeker;
			
			_rows = new RowCollection(wb);
			foreach(var indexPos in index.Rows)
			{
				long dbCellPos = indexPos;
				var dbCellIdx = records.IndexOfKey(dbCellPos);
				var dbCell = (DbCellRecord)records[dbCellPos];

				if(dbCell.RowOffset > 0)
				{
					var rowPos = dbCellPos - dbCell.RowOffset;
					var recIndex = records.IndexOfKey(rowPos);
					Debug.Assert(recIndex != -1);

					var record = records.Values[recIndex++];
					while(record is RowRecord)
					{
						var row = (RowRecord)record;
						var currentRow = new Row(Workbook, row);
						_rows.Add(row.RowNumber, currentRow);

						record = records.Values[recIndex++];
					}

					while(recIndex <= dbCellIdx)
					{
						if(!(record is CellRecord))
						{
							record = records.Values[recIndex++];
							continue;
						}

						var thecell = (CellRecord)record;
						var currentRow = _rows[thecell.Row];

						if(thecell is SingleColCellRecord)
						{
							var cell = (SingleColCellRecord)thecell;
							var val = cell.Value;
				
							var newCell = new Cell(Workbook, val);
							if(cell is RowColXfCellRecord)
							{
								var xfCell = (RowColXfCellRecord)cell;

								var style = Workbook.Styles[xfCell.Xf];
								Debug.Assert(style != null);
								newCell.Style = style;
							}
							currentRow.Cells.Add((byte)cell.Col, newCell);
						}
						else
						{
							var cells = (MultipleColCellRecord)thecell;
							for(var i = cells.FirstCol; i <= cells.LastCol; ++i)
							{
								var val = cells.GetValue(i);
								if(val != null)
								{
									Cell newCell = null;
									if(val is RkRec)
									{
										var rk = (RkRec)val;

										newCell = new Cell(Workbook, rk.Value);
										var style = Workbook.Styles[rk.Xf];
										Debug.Assert(style != null);
										newCell.Style = style;
									}
									else
										newCell = new Cell(Workbook, val);

									currentRow.Cells.Add((byte)i, newCell);
								}
							}
						}

						record = records.Values[recIndex++];
					}
				}
			}
		}

		/// <summary>
		/// The name of the worksheet.
		/// </summary>
		public string Name
		{
			get
			{
				return _name;
			}
		}

		/// <summary>
		/// The collection of rows in the worksheet.
		/// </summary>
		public RowCollection Rows
		{
			get
			{
				return _rows;
			}
		}

		/// <summary>
		/// The hyperlink table/collection in the worksheet.
		/// </summary>
		public HyperLinkCollection HyperLinks
		{
			get
			{
				return _hyperlinks;
			}
		}
	}
}
