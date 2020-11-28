using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid.ExampleApp
{
	internal class MyRowSupplier : IRowSupplier<MyRow>
	{
		public MyRow Get(int row)
		{
			return new MyRow();
		}
	}
}
