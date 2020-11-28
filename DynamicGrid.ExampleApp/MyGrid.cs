using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid.ExampleApp
{
	class MyGrid : Grid<MyRow>
	{
		public MyGrid()
		{
			RowSupplier = new MyRowSupplier();
			Columns = new[] {
				new MyColumn(),
				new MyColumn(),
				new MyColumn(),
				new MyColumn(),
				new MyColumn()
			};
		}
	}
}
