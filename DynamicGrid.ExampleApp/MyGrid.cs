using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DynamicGrid.ExampleApp
{
	class MyGrid : Grid<MyRow>
	{
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

		private void Step()
		{
			if (++_stopwatchCounter == 20)
			{
				Fps = 20 / _stopwatch.Elapsed.TotalSeconds;

				_stopwatch.Restart();
				_stopwatchCounter = 0;
			}

			InvalidateData();
		}
	}
}
