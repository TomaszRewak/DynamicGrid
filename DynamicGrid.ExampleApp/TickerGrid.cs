using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace DynamicGrid.ExampleApp
{
	internal sealed class TickerGrid : Grid
	{
		private readonly List<DateTime> _items = new List<DateTime>();

		private int _selectedRow;
		public int SelectedRow
		{
			get => _selectedRow;
			set
			{
				if (_selectedRow == value) return;
				InvalidateRowData(_selectedRow);
				_selectedRow = value;
				UpdateData();
				InvalidateRowData(_selectedRow);
				UpdateData();
			}
		}

		public TickerGrid()
		{
			Columns = new[] { 50, 150, 150 };

			_ = new DispatcherTimer(
				TimeSpan.FromSeconds(1),
				DispatcherPriority.Background,
				(e, a) => Step(),
				Dispatcher.CurrentDispatcher);
		}

		private void Step()
		{
			bool isSnappedToTheTop = _items.Count * RowHeight == -VerticalOffset;

			_items.Add(DateTime.Now);

			if (isSnappedToTheTop)
				VerticalOffset -= RowHeight;
		}

		protected override Cell GetCell(int rowIndex, int columnIndex)
		{
			var itemIndex = -rowIndex - 1;

			if (itemIndex < 0 || itemIndex >= _items.Count)
				return Cell.Empty;

			var cell = columnIndex switch
			{
				0 => new Cell(itemIndex.ToString(), Color.Gray),
				1 => new Cell(_items[itemIndex].ToLongTimeString(), Color.White),
				2 => new Cell(DateTime.Now.ToLongTimeString(), Color.White),
				_ => throw new NotImplementedException()
			};

			return rowIndex == SelectedRow
				? cell.Highlight(Color.Blue, 0.25)
				: cell;
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			VerticalOffset = Math.Max(-_items.Count * RowHeight, VerticalOffset - Math.Sign(e.Delta) * RowHeight);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			VerticalOffset = -_items.Count * RowHeight;
		}

		protected override void OnCellClicked(MouseCellEventArgs e)
		{
			SelectedRow = e.Row;
		}
	}
}
