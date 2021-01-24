using DynamicGrid.Utils;
using System;
using System.Collections.Generic;

namespace DynamicGrid.Data
{
	internal readonly struct ColumnPlacement
	{
		[ThreadStatic]
		private static readonly List<int> _columnsWidths = new List<int>();

		public readonly int CroppedIndex;
		public readonly int CroppedOffset;
		public readonly int RealOffset;
		public readonly int Width;
		public readonly int CroppedRowWidth;

		public int RealOffsetPlusWidth => RealOffset + Width;

		public ColumnPlacement(int croppedIndex, int croppedOffset, int realOffset, int width, int croppedRowWidth)
		{
			CroppedIndex = croppedIndex;
			CroppedOffset = croppedOffset;
			RealOffset = realOffset;
			Width = width;
			CroppedRowWidth = croppedRowWidth;
		}

		public static int GetColumnIndex(List<ColumnPlacement> columns, int offset, int hint)
		{
			if (columns.Count == 0) return 0;

			hint = MathUtils.Clip(0, hint, columns.Count - 1);

			if (offset >= columns[hint].RealOffset)
			{
				for (int c = hint; c < columns.Count; c++)
					if (offset < columns[c].RealOffsetPlusWidth)
						return c;

				return columns.Count - 1;
			}
			else
			{
				for (int c = hint - 1; c >= 0; c--)
					if (offset >= columns[c].RealOffset)
						return c;

				return 0;
			}
		}

		public static void CalculatePlacement(IEnumerable<int> columnWidths, int containerWidth, List<ColumnPlacement> columnPlacement)
		{
			_columnsWidths.Clear();
			foreach (var width in columnWidths)
				_columnsWidths.Add(width);

			columnPlacement.Clear();

			var croppedIndex = 0;
			var croppedOffset = 0;
			var realOffset = 0;
			var croppedRowWidth = 0;
			var maxVisibleColumnsWidth = CalculateMaxVisibleColumnsWidth(_columnsWidths, containerWidth);

			for (int realIndex = 0; realIndex < _columnsWidths.Count; realIndex++)
			{
				var width = _columnsWidths[realIndex];

				if (croppedOffset + width > maxVisibleColumnsWidth)
				{
					croppedIndex = 0;
					croppedOffset = 0;
					croppedRowWidth = 0;
				}

				if (croppedIndex == 0)
				{
					for (int inRowIndex = realIndex; inRowIndex < _columnsWidths.Count; inRowIndex++)
					{
						if (croppedRowWidth + _columnsWidths[inRowIndex] <= maxVisibleColumnsWidth)
							croppedRowWidth += _columnsWidths[inRowIndex];
						else
							break;
					}
				}

				columnPlacement.Add(new ColumnPlacement(croppedIndex, croppedOffset, realOffset, width, croppedRowWidth));

				croppedIndex++;
				croppedOffset += width;
				realOffset += width;
			}
		}

		private static int CalculateMaxVisibleColumnsWidth(List<int> columnWidths, int containerWidth)
		{
			var lastVisibleColumn = 0;
			var visibleColumnsWidth = 0;
			var maxVisibleColumnsWidth = 0;

			for (int column = 0; column < columnWidths.Count; column++)
			{
				while (lastVisibleColumn < column && visibleColumnsWidth - columnWidths[lastVisibleColumn] >= containerWidth)
				{
					visibleColumnsWidth -= columnWidths[lastVisibleColumn];
					lastVisibleColumn++;
				}

				visibleColumnsWidth += columnWidths[column];
				maxVisibleColumnsWidth = Math.Max(maxVisibleColumnsWidth, visibleColumnsWidth);
			}

			return maxVisibleColumnsWidth;
		}
	}
}
