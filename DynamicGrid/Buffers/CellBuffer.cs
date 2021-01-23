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

		private Size _size;
		public Size Size
		{
			get => _size;
			set
			{
				_size = value;

				if (value.Width <= Width && value.Height <= Height)
					return;

				_cells = new Cell[
					Math.Max(value.Height * 2, Height),
					Math.Max(value.Width * 2, Width)];
			}
		}

		public void Clear(Color color)
		{
			for (var y = 0; y < Height; y++)
				for (var x = 0; x < Width; x++)
					_cells[y, x] = new Cell(color);
		}

		public void ClearColumn(int index, Color color)
		{
			if (index >= Width) return;

			for (var y = 0; y < Height; y++)
				_cells[y, index] = new Cell(color);
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

		public int CropRow(int index)
		{
			return (index % Size.Height + Size.Height) % Size.Height;
		}
	}
}
