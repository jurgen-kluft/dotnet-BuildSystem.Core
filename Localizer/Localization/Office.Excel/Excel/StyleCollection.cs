using System;
using System.Diagnostics;

namespace Net.Office.Excel
{
	/// <summary>
	/// Collection class for Style objects.
	/// </summary>
	public class StyleCollection : ExcelCollection<Style>
	{
		internal StyleCollection(Workbook wb) : base(wb)
		{			
		}
	}
}
