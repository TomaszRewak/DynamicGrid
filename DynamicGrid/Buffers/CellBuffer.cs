using System;
using System.Diagnostics;
using System.Drawing;

namespace DynamicGrid.Buffers
{
	internal sealed class CellBuffer
	{
		private Cell[,] _cells = new Cell[0, 0];

		private int Height => _cells.GetLength(0);
		private int Width => _cells.GetLength(1);

		public void Clear(Color color)
		{
			for (var y = 0; y < Height; y++)
				for (var x = 0; x < Width; x++)
					_cells[y, x] = new Cell(color);
		}

		public void Resize(int width, int height)
		{
			if (width <= Width && height <= Height)
				return;

			_cells = new Cell[
				Math.Max(height, Height),
				Math.Max(width, Width)];
		}

		public bool TrySet(int row, int column, in Cell value)
		{
			Debug.Assert(column >= 0);
			Debug.Assert(column < Width);
			Debug.Assert(row >= 0);
			Debug.Assert(row < Height);

			ref var cell = ref _cells[row, column];
			var changed = cell != value;

			cell = value;

			return changed;
		}
	}
}
