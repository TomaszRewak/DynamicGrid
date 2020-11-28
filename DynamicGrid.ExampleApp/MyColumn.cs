using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid.ExampleApp
{
	internal class MyColumn : Column<MyRow>
	{
		public MyColumn() : base("my column")
		{ }

		public override Cell GetCell(MyRow row)
		{
			return new Cell("aaa", Color.Red);
		}
	}
}
