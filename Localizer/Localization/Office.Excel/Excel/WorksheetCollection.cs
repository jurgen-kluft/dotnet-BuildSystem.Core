using System;
using System.Collections.Generic;
using Net.SourceForge.Koogra.Collections;

namespace Net.Office.Excel
{
	/// <summary>
	/// Collection class of Worksheet objects.
	/// </summary>
	public class WorksheetCollection : SimpleCollection<Worksheet>
	{
		internal WorksheetCollection()
		{
		}

		/// <summary>
		/// Retrieves a worksheet given its name.
		/// </summary>
		/// <param name="index">The name of the worksheet.</param>
		/// <returns>Returns null if the worksheet cannot be found.</returns>
		/// <remarks>Search is case sensitive.</remarks>
		public Worksheet GetByName(string index)
		{
			foreach(Worksheet sheet in this)
			{
				if(string.Compare(sheet.Name, index, true) == 0)
					return sheet;
			}

			return null;
		}
	}
}
