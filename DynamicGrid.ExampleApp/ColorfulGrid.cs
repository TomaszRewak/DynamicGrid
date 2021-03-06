﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Threading;

namespace DynamicGrid.ExampleApp
{
	internal sealed class ColorfulGrid : Grid
	{
		private readonly Stopwatch _stopwatch;
		private int _stopwatchCounter;

		public double Fps { get; private set; }

		public ColorfulGrid()
		{
			_stopwatch = Stopwatch.StartNew();

			_ = new DispatcherTimer(
				TimeSpan.FromMilliseconds(1),
				DispatcherPriority.Background,
				(e, a) => Step(),
				Dispatcher.CurrentDispatcher);
		}

		protected override Cell GetCell(int rowIndex, int columnIndex)
		{
			var now = DateTime.Now;

			var color = Color.FromArgb(
				(int)((1 + Math.Sin((double)now.Ticks / 10000000 + columnIndex * 0.05 + rowIndex * 0.05)) / 2 * 255),
				(int)((1 + Math.Sin((double)now.Ticks / 20000000 + columnIndex * 0.1 + rowIndex * 0.2)) / 2 * 255),
				(int)((1 + Math.Sin((double)now.Ticks / 30000000 + columnIndex * 0.5 + rowIndex * 0.05)) / 2 * 255));

			var alignemnt = (columnIndex % 3) switch
			{
				0 => HorizontalAlignment.Left,
				1 => HorizontalAlignment.Center,
				_ => HorizontalAlignment.Right
			};

			var style = (rowIndex % 5) switch
			{
				0 => FontStyle.Regular,
				1 => FontStyle.Bold,
				2 => FontStyle.Italic,
				3 => FontStyle.Strikeout,
				_ => FontStyle.Underline
			};

			return new Cell($"{Fps:####}fps X {now.Millisecond:D3} {rowIndex} {columnIndex}", color, Color.Black, alignemnt, style);
		}

		private void Step()
		{
			if (++_stopwatchCounter == 20)
			{
				Fps = 20 / _stopwatch.Elapsed.TotalSeconds;

				_stopwatch.Restart();
				_stopwatchCounter = 0;
			}

			InvalidateCellData(-10, 3);
			InvalidateCellData(10, 3);
			InvalidateCellData(15, 2);
			UpdateData();
			Update();
		}
	}
}
