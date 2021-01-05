using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynamicGrid.ExampleApp
{
	public partial class Form1 : Form
	{
		private readonly MyDataSupplier _dataSupplier = new MyDataSupplier();

		public Form1()
		{
			InitializeComponent();

			var columns = Enumerable.Range(0, 20).Select(c => new MyColumn(_grid, c)).ToArray();

			_gridHeader.Columns = columns;
			_grid.ColumnWidths = columns;

			_grid.DataSupplier = _dataSupplier;
		}

		private void OnHorizontalScrollBarValueChanged(object sender, EventArgs e)
		{
			_grid.HorizontalOffset = _horizontalScrollBar.Value;
			_gridHeader.HorizontalOffset = _horizontalScrollBar.Value;
		}

		private void OnVerticalScrollBarValueChanged(object sender, EventArgs e)
		{
			_dataSupplier.Offset = _verticalScrollBar.Value;
			_grid.InvalidateData();
		}

		private void OnColumnsWidthChanged(object sender, EventArgs e)
		{
			_horizontalScrollBar.Maximum = Math.Max(0, _gridHeader.ColumnsWidths);
		}

		private void OnColumnsChanged(object sender, EventArgs e)
		{
			_grid.ColumnWidths = _gridHeader.Columns;
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);

			_horizontalScrollBar.LargeChange = ClientSize.Width - _verticalScrollBar.Width;
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);

			_verticalScrollBar.Value = Math.Max(_verticalScrollBar.Minimum, Math.Min(_verticalScrollBar.Maximum, _verticalScrollBar.Value - Math.Sign(e.Delta)));
		}
	}
}
