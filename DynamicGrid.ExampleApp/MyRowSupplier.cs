using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid.ExampleApp
{
	internal class MyRowSupplier : IRowSupplier<MyRow>
	{
		List<MyRow> _rows = Enumerable.Range(0, 1000).Select(r => new MyRow(r)).ToList();

		public MyRow Get(int row)
		{
			if (row >= 0 && row < _rows.Count)
				return _rows[row];

			return null;
		}
	}
}
