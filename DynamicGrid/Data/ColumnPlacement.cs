using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid.Data
{
	internal readonly struct ColumnPlacement
	{
		public readonly int CroppedIndex;
		public readonly int CroppedOffset;
		public readonly int RealOffset;

		public ColumnPlacement(int croppedIndex, int croppedOffset, int realOffset)
		{
			CroppedIndex = croppedIndex;
			CroppedOffset = croppedOffset;
			RealOffset = realOffset;
		}

		public static void CalculatePlacement(List<int> columnWidths, int containerWidth, List<ColumnPlacement> columnPlacement)
		{
			columnPlacement.Clear();

			var croppedIndex = 0;
			var croppedOffset = 0;
			var realOffset = 0;
			var maxVisibleColumnsWidth = CalculateMaxVisibleColumnsWidth(columnWidths, containerWidth);

			foreach (var width in columnWidths)
			{
				if (croppedOffset + width > maxVisibleColumnsWidth)
				{
					croppedIndex = 0;
					croppedOffset = 0;
				}

				columnPlacement.Add(new ColumnPlacement(croppedIndex, croppedOffset, realOffset));

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
