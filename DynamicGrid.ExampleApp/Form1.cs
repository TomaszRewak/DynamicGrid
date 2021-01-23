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

			_gridHeader.Columns.AddRange(Enumerable.Range(0, 200).Select(c => new DataGridViewTextBoxColumn()
			{
				HeaderText = $"Column {c}",
				Width = 150
			}).ToArray());
		}

		private void OnHorizontalScrollBarValueChanged(object sender, EventArgs e)
		{
			_grid.HorizontalOffset = _horizontalScrollBar.Value;
			_gridHeader.HorizontalScrollingOffset = Math.Max(0, _horizontalScrollBar.Value);
		}

		private void OnColumnsChanged(object sender, DataGridViewColumnEventArgs e)
		{
			_grid.Columns = _gridHeader.Columns.OfType<DataGridViewColumn>().Select(c => c.Width);
			_horizontalScrollBar.Maximum = _grid.Columns.Sum() + 500;
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
