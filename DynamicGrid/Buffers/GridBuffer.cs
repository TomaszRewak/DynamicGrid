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
			var columnIndex = 0;
			var columnOffset = 0;

			foreach (var column in columns)
			{
				var rowIndex = 0;
				var rowOffset = 0;

				foreach (var row in rows)
				{
					var cell = column.GetValue(row);
					var changed = _cellBuffer.TrySet(rowIndex, columnIndex, in cell);

					if (changed)
						_displayBuffer.Draw()
				}
			}

			Debug.Assert(_cells.GetLength(0) >= rows.Length);
			Debug.Assert(_cells.GetLength(1) >= columns.Length);

			int minColumn = int.MaxValue,
				maxColumn = int.MinValue,
				minRow = int.MaxValue,
				maxRow = int.MinValue;

			for (var row = 0; row < rows.Length; row++)
			{
				for (var column = 0; column < columns.Length; column++)
				{
					var newState = columns[column].GetValue(rows[row]);
					ref var cell = ref _cells[row, column];

					cell.State = newState;
					cell.Changed = newState != cell.State;

					if (cell.Changed)
					{
						minColumn = Math.Min(minColumn, column);
						maxColumn = Math.Max(maxColumn, column);
						minRow = Math.Min(minRow, row);
						maxRow = Math.Max(maxRow, row);
					}
				}
			}

			return new Rectangle(
				minColumn,
				minRow,
				maxColumn - minColumn,
				maxRow - minRow);
		}
	}
}
