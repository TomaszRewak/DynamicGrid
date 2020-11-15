﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid.Buffer
{
	internal sealed class CellBuffer
	{
		private (bool Changed, Cell State)[,] _cells;

		private int Height => _cells.GetLength(0);
		private int Width => _cells.GetLength(1);

		public Rectangle Update<TRow>(ReadOnlySpan<IColumn<TRow>> columns, ReadOnlySpan<TRow> rows)
		{
			Debug.Assert(Width >= columns.Length);
			Debug.Assert(Height >= rows.Length);

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
