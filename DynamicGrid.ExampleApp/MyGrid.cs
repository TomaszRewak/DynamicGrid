using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

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

			_ = new DispatcherTimer(
				TimeSpan.FromMilliseconds(10),
				DispatcherPriority.Background,
				(e, a) => InvalidateData(),
				Dispatcher.CurrentDispatcher);
		}
	}
}
