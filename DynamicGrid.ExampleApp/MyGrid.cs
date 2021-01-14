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
	internal sealed class MyGrid : Grid
	{
		private readonly List<MyRow> _rows;
		private readonly List<MyColumn> _columns;
		private readonly Stopwatch _stopwatch;
		private int _stopwatchCounter;

		public double Fps { get; private set; }

		public MyGrid()
		{
			_rows = Enumerable.Range(0, 100).Select(i => new MyRow(i)).ToList();
			_columns = Enumerable.Range(0, 100).Select(i => new MyColumn(this, i)).ToList();
			_stopwatch = Stopwatch.StartNew();

			_ = new DispatcherTimer(
				TimeSpan.FromMilliseconds(1),
				DispatcherPriority.Background,
				(e, a) => Step(),
				Dispatcher.CurrentDispatcher);
		}

		public override Cell GetCell(int rowIndex, int columnIndex)
		{
			if (rowIndex < 0 || rowIndex >= _rows.Count)
				return Cell.Empty;

			return _columns[columnIndex].GetCell(_rows[rowIndex]);
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
