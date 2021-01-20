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

		public ColumnPlacement(int croppedIndex, int croppedOffset, int realOffset, int width)
		{
			CroppedIndex = croppedIndex;
			CroppedOffset = croppedOffset;
			RealOffset = realOffset;
			Width = width;
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
			var maxVisibleColumnsWidth = CalculateMaxVisibleColumnsWidth(_columnsWidths, containerWidth);

			foreach (var width in _columnsWidths)
			{
				if (croppedOffset + width > maxVisibleColumnsWidth)
				{
					croppedIndex = 0;
					croppedOffset = 0;
				}

				columnPlacement.Add(new ColumnPlacement(croppedIndex, croppedOffset, realOffset, width));

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
