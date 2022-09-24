using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Net.Office.Excel.Records;

namespace Net.Office.Excel
{
	/// <summary>
	/// Represents a color palette.
	/// </summary>
	public class Palette : ExcelObject
	{
		private const ushort _minUserDefined = 0x0008;
		private const ushort _maxUserDefined = 0x003f;

		private static readonly int[] _defaultPaletteExcel2003 = new int[] {
			0x000000, 0xFFFFFF, 0xFF0000, 0x00FF00, 0x0000FF, 0xFFFF00, 0xFF00FF, 0x00FFFF, 
			0x800000, 0x008000, 0x000080, 0x808000, 0x800080, 0x008080, 0xBBBBBB, 0x808080, 
			0x000080, 0xBB80BB, 0xFFFFBB, 0xBBFFFF, 0x600060, 0xFF0080, 0x0000BB, 0xBBBBFF, 
			0x000050, 0xFF00FF, 0xFFFF00, 0x00FFFF, 0x700070, 0x800000, 0x008080, 0x0000FF, 
			0x3366FF, 0x33CCFF, 0xCCFFCC, 0xFFFFCC, 0xCCCCFF, 0xFFCCCC, 0xFFCCFF, 0x800080, 
			0x003366, 0x339999, 0x339933, 0xFF8000, 0xBB6600, 0x993300, 0x333399, 0x555555,
			0x000055, 0x009900, 0x003300, 0x333300, 0x000000, 0x993399, 0x333399, 0x333333,
			0x000000, 0x000000, 0x000000, 0x000000, 0x993300, 0x000000, 0x000000, 0x000000,
		};

		private PaletteEntry[] _builtIn;
		private IDictionary<int, PaletteEntry> _others;
		private PaletteEntry[] _colors;

		internal Palette(Workbook wb) : base(wb)
		{
			_builtIn = new PaletteEntry[] { new PaletteEntry(wb, Color.Black), new PaletteEntry(wb, Color.White), 
				new PaletteEntry(wb, Color.Red), new PaletteEntry(wb, Color.Green), 
				new PaletteEntry(wb, Color.Blue), new PaletteEntry(wb, Color.Yellow), 
				new PaletteEntry(wb, Color.Magenta), new PaletteEntry(wb, Color.Cyan)
										  };

			_others = new Dictionary<int, PaletteEntry>(3);
			_others[0x0040] = new PaletteEntry(wb, Color.FromKnownColor(KnownColor.ActiveBorder));
			_others[0x0041] = new PaletteEntry(wb, Color.FromKnownColor(KnownColor.Window));
			_others[0x7FFF] = new PaletteEntry(wb, Color.FromKnownColor(KnownColor.WindowText));

			_colors = new PaletteEntry[64];
			int idx = 0;
			foreach(int c in _defaultPaletteExcel2003)
                _colors[idx++] = new PaletteEntry(wb, Color.FromArgb((int)((uint)0xFF000000 | (uint)c)));
		}

		internal void Initialize(PaletteRecord p)
		{
			int idx = _minUserDefined;

			foreach(int c in p.Colors)
			{
				PaletteEntry pe = _colors[idx++];

				if(pe != null)
					pe.Color = Color.FromArgb(c);
				else
					break;
			}
		}

		/// <summary>
		/// Gets a color in the palette.
		/// </summary>
		/// <param name="index">The index for the color.</param>
		/// <returns>Returns a PaletteEntry object.</returns>
		public PaletteEntry GetColor(int index)
		{
			if(index >= 0 && index < _builtIn.Length)
				return _builtIn[index];

            PaletteEntry entry;
            _others.TryGetValue(index, out entry);

			if(entry == null)
			{
				int entryIndex = index - _minUserDefined;

				if(entryIndex >= 0 && entryIndex < _colors.Length)
					entry = _colors[entryIndex];
			}

			return entry;
		}
	}
}
