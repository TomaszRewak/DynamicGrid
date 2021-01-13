using DynamicGrid.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynamicGrid.Buffers
{
	public class DrawingContext
	{
		private readonly IntPtr _buffer;
		private readonly Cell[,] _cells;
		private readonly List<ColumnPlacement> _columns;
		private readonly int _rows;
		private readonly int _rowHeight;
		private readonly Color _backgroundColor;

		private Color? _currentColor;
		private HorizontalAlignment? _currentAlignemnt;

		public Rectangle InvalidatedRect { get; private set; } = Rectangle.Empty;

		public DrawingContext(IntPtr buffer, Cell[,] cells, List<ColumnPlacement> columns)
		{
			_buffer = buffer;
			_cells = cells;
			_columns = columns;
		}

		public void Draw(int rowIndex, int columnIndex, in Cell cell)
		{
			Debug.Assert(columnIndex >= 0);
			Debug.Assert(columnIndex < _columns.Count);

			var croppedRowIndex = (rowIndex % _rows + _rows) % _rows;
			var croppedColumnIndex = _columns[columnIndex].CroppedIndex;

			if (_cells[croppedRowIndex, croppedColumnIndex] == cell) return;
			_cells[croppedRowIndex, croppedColumnIndex] = cell;

			var rectangle = new Rectangle(
				_columns[columnIndex].CroppedOffset,
				_rowHeight * croppedRowIndex,
				_columns[columnIndex].Width - 1,
				_rowHeight - 1);
			var position = new Point(rectangle.Width / 2, 0);
			var cellColor = cell.Color.A == byte.MaxValue
				? cell.Color
				: _backgroundColor;

			if (_currentColor != cellColor)
			{
				_currentColor = cellColor;
				Gdi32.SetBackgroundColor(_buffer, cellColor);
			}

			if (_currentAlignemnt != cell.Alignment)
			{
				_currentAlignemnt = cell.Alignment;
				Gdi32.SetTextAlignemnt(_buffer, cell.Alignment);
			}

			Gdi32.PrintText(_buffer, rectangle, position, cell.Text);

			InvalidatedRect = InvalidatedRect == Rectangle.Empty // << TODO
				? rectangle
				: Rectangle.Union(InvalidatedRect, rectangle);
		}
	}
}
