using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid.Buffers
{
	internal sealed class GridBuffer<TRow>
	{
		private readonly CellBuffer _cellBuffer;
		private readonly DisplayBuffer<TRow> _displayBuffer;

		public GridBuffer(IntPtr graphics)
		{
			_cellBuffer = new CellBuffer();
			_displayBuffer = new DisplayBuffer<TRow>(graphics);
		}

		public Rectangle Update(ReadOnlySpan<IColumn<TRow>> columns, ReadOnlySpan<TRow> rows)
		{
			const int rowHeight = 20;
			const int columnWidth = 70;

			var columnIndex = 0;
			var columnOffset = 0;

			int minColumnOffset = int.MaxValue,
				maxColumnOffset = int.MinValue,
				minRowOffset = int.MaxValue,
				maxRowOffset = int.MinValue;

			foreach (var column in columns)
			{
				var rowIndex = 0;
				var rowOffset = 0;

				foreach (var row in rows)
				{
					var cell = column.GetValue(row);
					var changed = _cellBuffer.TrySet(rowIndex, columnIndex, in cell);

					if (changed)
					{
						_displayBuffer.Draw(columnOffset, rowOffset, columnWidth, rowHeight, cell);

						minColumnOffset = Math.Min(minColumnOffset, columnOffset);
						maxColumnOffset = Math.Max(maxColumnOffset, columnOffset + columnWidth);
						minRowOffset = Math.Min(minRowOffset, rowOffset);
						maxRowOffset = Math.Max(maxRowOffset, rowOffset + rowHeight);
					}

					rowIndex++;
					rowOffset += rowHeight;
				}

				columnIndex++;
				columnOffset += columnWidth;
			}

			return new Rectangle(
				minColumnOffset,
				minRowOffset,
				maxColumnOffset - minColumnOffset,
				maxRowOffset - minRowOffset);
		}
	}
}
