using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DynamicGrid.ExampleApp
{
	internal sealed class MyGrid : Grid
	{
		private readonly List<MyRow> _rows = Enumerable.Range(0, 200).Select(r => new MyRow(r)).ToList();
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

		public int Offset { get; set; }

		override public Cell GetCell(int row, int column)
		{
			row += Offset;

			if (row >= 0 && row < _rows.Count)
				return _rows[row];

			return null;
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
