using DynamicGrid.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DynamicGrid.Data
{
	internal readonly struct SectorPlacement
	{
		public readonly int Width;
		public readonly int Offset;

		public SectorPlacement(int width, int offset)
		{
			Width = width;
			Offset = offset;
		}
	}

	internal readonly struct ColumnPlacement
	{
		public readonly int Width;
		public readonly int GlobalOffset;
		public readonly int Sector;
		public readonly int SectorOffset;
		public readonly int SectorIndex;

		public int GlobalOffsetPlusWidth => GlobalOffset + Width;
		public int SectorOffsetPlusWidth => SectorOffset + Width;

		public ColumnPlacement(int width, int globalOffset, int sector, int sectorOffset, int sectorIndex)
		{
			Width = width;
			GlobalOffset = globalOffset;
			Sector = sector;
			SectorOffset = sectorOffset;
			SectorIndex = sectorIndex;
		}
	}

	internal static class ColumnPlacementResolver
	{
		public static int GetColumnIndex(List<ColumnPlacement> columns, int offset, int hint)
		{
			if (columns.Count == 0) return 0;

			hint = MathUtils.Clip(0, hint, columns.Count - 1);

			if (offset >= columns[hint].GlobalOffset)
			{
				for (int c = hint; c < columns.Count; c++)
					if (offset < columns[c].GlobalOffsetPlusWidth)
						return c;

				return columns.Count - 1;
			}
			else
			{
				for (int c = hint - 1; c >= 0; c--)
					if (offset >= columns[c].GlobalOffset)
						return c;

				return 0;
			}
		}

		public static void CalculatePlacement(IEnumerable<int> columnWidths, int containerWidth, List<ColumnPlacement> columnPlacement, List<SectorPlacement> sectorPlacement)
		{
			containerWidth = Math.Max(containerWidth, 1);

			columnPlacement.Clear();
			sectorPlacement.Clear();

			var columnWidthsEnumerator = columnWidths.GetEnumerator();
			if (!columnWidthsEnumerator.MoveNext()) return;

			columnPlacement.Add(new ColumnPlacement(columnWidthsEnumerator.Current, 0, 0, 0, 0));

			var sectorOffset = 0;
			var lastVisibleColumn = 0;

			while (columnWidthsEnumerator.MoveNext())
			{
				var width = columnWidthsEnumerator.Current;

				if (columnPlacement[^1].SectorOffsetPlusWidth + sectorOffset >= containerWidth)
				{
					sectorPlacement.Add(new SectorPlacement(columnPlacement[^1].SectorOffsetPlusWidth, sectorOffset));
					columnPlacement.Add(new ColumnPlacement(width, columnPlacement[^1].GlobalOffsetPlusWidth, sectorPlacement.Count, 0, 0));
					sectorOffset = 0;
				}
				else
				{
					columnPlacement.Add(new ColumnPlacement(width, columnPlacement[^1].GlobalOffsetPlusWidth, sectorPlacement.Count, columnPlacement[^1].SectorOffsetPlusWidth, columnPlacement[^1].SectorIndex + 1));
				}

				while (columnPlacement[^1].GlobalOffset - columnPlacement[lastVisibleColumn].GlobalOffsetPlusWidth >= containerWidth)
				{
					lastVisibleColumn++;
				}

				if (columnPlacement[lastVisibleColumn].Sector < columnPlacement[^1].Sector)
				{
					sectorOffset = Math.Min(sectorOffset, columnPlacement[lastVisibleColumn].SectorOffset + sectorPlacement[^1].Offset - columnPlacement[^1].SectorOffsetPlusWidth);
				}
			}

			sectorPlacement.Add(new SectorPlacement(columnPlacement[^1].SectorOffsetPlusWidth, sectorOffset));

			NormalizeSectorPlacement(sectorPlacement);
		}

		private static void NormalizeSectorPlacement(List<SectorPlacement> sectorPlacement)
		{
			var minOffset = sectorPlacement.Min(s => s.Offset);

			for (var i = 0; i < sectorPlacement.Count; i++)
				sectorPlacement[i] = new SectorPlacement(sectorPlacement[i].Width, sectorPlacement[i].Offset - minOffset);
		}
	}
}
