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
		public Form1()
		{
			InitializeComponent();

			var columns = Enumerable.Range(0, 20).Select(c => new MyColumn(_grid, c)).ToArray();

			_gridHeader.Columns = columns;
			_grid.Columns = columns;
		}

		private void OnHorizontalScrollBarValueChanged(object sender, EventArgs e)
		{
			_grid.HorizontalOffset = _horizontalScrollBar.Value;
		}
	}
}
