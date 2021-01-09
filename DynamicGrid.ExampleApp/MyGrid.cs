using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace DynamicGrid.ExampleApp
{
	internal sealed class MyGrid : Grid<MyRow>
	{
		private readonly List<MyRow> _rows = Enumerable.Range(0, 100).Select(i => new MyRow(i)).ToList();
		private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
		private int _stopwatchCounter;

		public double Fps { get; private set; }

		public MyGrid()
		{
			_ = new DispatcherTimer(
				TimeSpan.FromMilliseconds(1),
				DispatcherPriority.Background,
				(e, a) => Step(),
				Dispatcher.CurrentDispatcher);
		}

		public override MyRow GetRow(int rowIndex)
		{
			return _rows[rowIndex];
		}

		public override bool ValidateRow(int rowIndex)
		{
			return rowIndex >= 0 && rowIndex < _rows.Count;
		}

		private void Step()
		{
			if (++_stopwatchCounter == 20)
			{
				Fps = 20 / _stopwatch.Elapsed.TotalSeconds;

				_stopwatch.Restart();
				_stopwatchCounter = 0;
			}

			//InvalidateData();
			//InvalidateColumn(2);
			//InvalidateColumn(4);
			//InvalidateRow(5);
			//InvalidateRow(7);
			InvalidateCell(10, 5);
			//InvalidateColumn(Columns.ElementAt(3));
		}
	}
}
