using DynamicGrid.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynamicGrid.Managers
{
	internal sealed class MouseManager
	{
		public enum Events
		{
			None = 0,
			Click = 1,
			DoubleClick = 2,
			MouseDown = 4,
			MouseUp = 8,
			MouseMove = 16,
			MouseEnter = 32,
			MouseLeave = 64
		}

		private readonly Grid _grid;
		private readonly List<ColumnPlacement> _columns;

		public Events PendingEvents { get; private set; }

		private int _x;
		private int _y;
		private int _row;
		private int _column;
		private bool _isMouseOver;
		private bool _isMouseDown;

		public MouseManager(Grid grid, List<ColumnPlacement> columns)
		{
			_grid = grid;
			_columns = columns;
		}

		public Events OnClick(MouseEventArgs e)
		{
			if (!IsPointInsideGrid(e))
				return Events.None;

			return Process(e) | Events.Click;
		}

		public Events UpdateMousePosition(int x, int y)

		public Events Refresh()
		{
			x += _grid.HorizontalOffset;
			y += _grid.VerticalOffset;

			var column = ColumnPlacement.GetColumnIndex(_columns, x, _column);
			var row = y >= 0
				? y / _grid.RowHeight
				: (y - _grid.RowHeight + 1) / _grid.RowHeight;
			var isMouseOver =
				_columns.Count >= 0 &&
				x >= -_grid.HorizontalOffset &&
				x < _columns[_columns.Count - 1].RealOffsetPlusWidth;

			if (isMouseOver && !_isMouseOver)

			if (row == _row && column == _column)
				return Events.None;

			_row = row;
			_column = column;

			return Events.MouseMove;
		}

		private bool IsPointInsideGrid(MouseEventArgs e)
		{
			if (_columns.Count == 0) return false;
			if (e.X < -_grid.HorizontalOffset) return false;
			if (e.X >= _columns[_columns.Count - 1].RealOffsetPlusWidth) return false;

			return true;
		}
	}
}
