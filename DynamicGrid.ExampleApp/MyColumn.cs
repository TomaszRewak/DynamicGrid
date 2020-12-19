using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynamicGrid.ExampleApp
{
	internal class MyColumn : Column<MyRow>
	{
		private readonly MyGrid _grid;
		private readonly int _index;

		public MyColumn(MyGrid grid, int index) : base("my column", 100)
		{
			_grid = grid;
			_index = index;
		}

		public override Cell GetCell(MyRow row)
		{
			if (row == null)
				return Cell.Empty;

			var now = DateTime.Now;

			//if (rowIndex % 10 > 1 || _index % 10 > 1)
			//	return new Cell(Color.Gray);

			var color = Color.FromArgb(
				(int)((1 + Math.Sin((double)now.Ticks / 10000000 + _index * 0.05 + row.Index * 0.05)) / 2 * 255),
				(int)((1 + Math.Sin((double)now.Ticks / 20000000 + _index * 0.1 + row.Index * 0.2)) / 2 * 255),
				(int)((1 + Math.Sin((double)now.Ticks / 30000000 + _index * 0.5 + row.Index * 0.05)) / 2 * 255));

			return new Cell($"{_grid.Fps:####}fps X {now.Millisecond:D3}", HorizontalAlignment.Center, color);
		}
	}
}
