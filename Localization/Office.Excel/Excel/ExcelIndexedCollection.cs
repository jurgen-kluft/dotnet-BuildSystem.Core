using System;
using Net.SourceForge.Koogra.Collections;

namespace Net.Office.Excel
{
	/// <summary>
	/// Base class for indexed collections that are part of a Workbook
	/// </summary>
	public abstract class ExcelIndexedCollection<TKey, TValue> : IndexedCollection<TKey, TValue>, IExcelObject
        where TKey : IComparable<TKey>
	{
		private Workbook _wb;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="wb">The workbook.</param>
		/// <exception cref="ArgumentNullException">Exception thrown if wb is null.</exception>
		protected ExcelIndexedCollection(Workbook wb)
		{		
			if(wb == null)
				throw new ArgumentNullException("wb");

			_wb = wb;
		}

		/// <summary>
		/// The workbook.
		/// </summary>
		public Workbook Workbook
		{
			get
			{
				return _wb;
			}
		}
	}
}
