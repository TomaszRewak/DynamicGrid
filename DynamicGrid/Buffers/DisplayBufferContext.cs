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
	internal sealed partial class GridBuffer
	{
		public ref struct DrawingContext
		{
			private readonly ReadOnlySpan<ColumnPlacement> _columns;
			private readonly int _columnOffset;
			private readonly int _rowOffset;

			private Color? _currentColor;
			private HorizontalAlignment? _currentAlignemnt;

			public Rectangle InvalidatedRect { get; private set; } = Rectangle.Empty;

			public DrawingContext(GridBuffer grid)
			{
				_columnWidths = grid.ColumnWidths;

			}

			public void Draw(int column, int row, in Cell cell)
			{
				Debug.Assert(column >= 0);
				Debug.Assert(column < ColumnWidths.Length);
				Debug.Assert(row * RowHeight < VerticalOffset + ParentSize.Height);
				Debug.Assert(row * RowHeight + RowHeight > VerticalOffset);

				column = _columnOffsets[column].CellOffset;
				row = (row % VisibleRows + VisibleRows) % VisibleRows;

				if (_cells[row, column] == value) return;
				_cells[row, column] = value;

				ref var cell = ref _cells[row, column];
				var changed = cell != value;

				cell = value;

				var rectangle = new Rectangle(x, y, width - 1, height - 1);
				var position = new Point(width / 2, 0);
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

				InvalidatedRect = InvalidatedRect == Rectangle.Empty
					? rectangle
					: Rectangle.Union(InvalidatedRect, rectangle);
			}
		}
	}
}
