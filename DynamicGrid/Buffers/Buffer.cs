using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid.Buffers
{
	internal sealed class Buffer
	{
		private (bool Changed, Cell State)[,] _cells = new (bool Changed, Cell State)[0, 0];
		private BufferedGraphics _graphics;

		public Buffer()
		{
			_graphics.Graphics.GetHdc();
			_graphics.
		}

		public Rectangle Update<TRow>(ReadOnlySpan<IColumn<TRow>> columns, ReadOnlySpan<TRow> rows)
		{
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
