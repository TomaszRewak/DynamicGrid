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
		private readonly MyRowSupplier _rowSupplier = new MyRowSupplier();

		public Form1()
		{
			InitializeComponent();

			var columns = Enumerable.Range(0, 20).Select(c => new MyColumn(_grid, c)).ToArray();

			_gridHeader.Columns = columns;
			_grid.Columns = columns;

			_grid.RowSupplier = _rowSupplier;
		}

		private void OnHorizontalScrollBarValueChanged(object sender, EventArgs e)
		{
			_grid.HorizontalOffset = _horizontalScrollBar.Value;
			_gridHeader.HorizontalOffset = _horizontalScrollBar.Value;
		}

		private void OnVerticalScrollBarValueChanged(object sender, EventArgs e)
		{

		}

		private void OnColumnsWidthChanged(object sender, EventArgs e)
		{
			_horizontalScrollBar.Maximum = Math.Max(0, _gridHeader.ColumnsWidths);
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);

			_horizontalScrollBar.LargeChange = ClientSize.Width - _verticalScrollBar.Width;
		}
	}
}
