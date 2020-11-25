using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid.ExampleApp
{
	class DataSupplier : IDataSupplier
	{
		public Cell Get(int row, int column)
		{
			return new Cell($"{row}:{column}", Color.Red);
		}
	}
}
