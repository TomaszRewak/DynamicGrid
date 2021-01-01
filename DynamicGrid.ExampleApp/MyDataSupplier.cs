using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid.ExampleApp
{
	internal class MyDataSupplier : IDataSupplier<MyRow>
	{
		List<MyRow> _rows = Enumerable.Range(0, 200).Select(r => new MyRow(r)).ToList();

		public int Offset { get; set; }

		public MyRow Get(int row)
		{
			row += Offset;

			if (row >= 0 && row < _rows.Count)
				return _rows[row];

			return null;
		}
	}
}
