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
		private readonly Stopwatch _stopwatch;
		private int _stopwatchCounter;

		public double Fps { get; private set; }

		public MyGrid()
		{
			_stopwatch = Stopwatch.StartNew();

			_ = new DispatcherTimer(
				TimeSpan.FromMilliseconds(1),
				DispatcherPriority.Background,
				(e, a) => Step(),
				Dispatcher.CurrentDispatcher);
		}

		public override Cell GetCell(int rowIndex, int columnIndex)
		{
			var now = DateTime.Now;

			var color = Color.FromArgb(
				(int)((1 + Math.Sin((double)now.Ticks / 10000000 + columnIndex * 0.05 + rowIndex * 0.05)) / 2 * 255),
				(int)((1 + Math.Sin((double)now.Ticks / 20000000 + columnIndex * 0.1 + rowIndex * 0.2)) / 2 * 255),
				(int)((1 + Math.Sin((double)now.Ticks / 30000000 + columnIndex * 0.5 + rowIndex * 0.05)) / 2 * 255));

			return new Cell($"{Fps:####}fps X {now.Millisecond:D3} {rowIndex} {columnIndex}", HorizontalAlignment.Center, color);
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
			InvalidateCellData(-10, 3);
			InvalidateCellData(10, 3);
			InvalidateCellData(15, 2);
			//InvalidateColumn(Columns.ElementAt(3));
			UpdateData();
			Update();

			VerticalOffset += 1;
		}
	}
}
