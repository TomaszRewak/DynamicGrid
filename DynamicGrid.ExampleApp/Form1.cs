using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace DynamicGrid.ExampleApp
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();

			var columns = Enumerable.Range(0, 20).Select(c => new MyColumn(_grid, c)).ToArray();

			_gridHeader.Columns = columns;
		}

		private void OnHorizontalScrollBarValueChanged(object sender, EventArgs e)
		{
			_grid.HorizontalOffset = _horizontalScrollBar.Value;
			_gridHeader.HorizontalOffset = _horizontalScrollBar.Value;
		}

		private void OnTotalColumnWidthChanged(object sender, EventArgs e)
		{
			_horizontalScrollBar.Maximum = Math.Max(0, _gridHeader.TotalColumnWdith);
		}

		private void OnColumnsChanged(object sender, EventArgs e)
		{
			_grid.Columns = _gridHeader.Columns.Select(c => c.Width);
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);

			_horizontalScrollBar.LargeChange = ClientSize.Width;
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);

			_grid.VerticalOffset -= Math.Sign(e.Delta) * 4;
		}
	}
}
