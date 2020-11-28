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
		public MyColumn() : base("my column", 100)
		{ }

		public override Cell GetCell(MyRow row)
		{
			var now = DateTime.Now;

			return new Cell($"{now.Second:D2}:{now.Millisecond:D3}", Color.Red);
		}
	}
}
