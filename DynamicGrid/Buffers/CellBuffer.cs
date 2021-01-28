using System.Diagnostics;
using System.Drawing;

namespace DynamicGrid.Buffers
{
	internal sealed class CellBuffer
	{
		private Cell[,] _cells = new Cell[0, 0];

		public Size Capacity => new Size(_cells.GetLength(1), _cells.GetLength(0));

		private Size _size;
		public Size Size
		{
			get => _size;
			set
			{
				_size = value;

				if (value.Width <= Capacity.Width &&
					value.Width >= Capacity.Width / 4 &&
					value.Height <= Capacity.Height &&
					value.Height >= Capacity.Height / 4)
					return;

				_cells = new Cell[value.Height * 2, value.Width * 2];
			}
		}

		public void Clear(Color color)
		{
			for (var y = 0; y < Size.Height; y++)
				for (var x = 0; x < Size.Width; x++)
					_cells[y, x] = new Cell(backgroundColor: color);
		}

		public void ClearColumn(int index, Color color)
		{
			if (index < 0) return;
			if (index >= Size.Width) return;

			for (var y = 0; y < Size.Height; y++)
				_cells[y, index] = new Cell(backgroundColor: color);
		}

		public bool TrySet(int row, int column, in Cell value)
		{
			Debug.Assert(column >= 0);
			Debug.Assert(column < Capacity.Width);
			Debug.Assert(row >= 0);
			Debug.Assert(row < Capacity.Height);

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
