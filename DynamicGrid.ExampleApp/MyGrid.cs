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

			return new Cell($"{Fps:####}fps X {now.Millisecond:D3} {rowIndex} {columnIndex}", color, alignemnt, style);
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
			InvalidateColumnData(7);
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

		protected override void OnCellClicked(MouseCellEventArgs e)
		{
			base.OnCellClicked(e);

			Trace.WriteLine($"{e.Row}:{e.Coulmn} click\t\t{e.GridRect.X}:{e.GridRect.Y}:{e.GridRect.Width}:{e.GridRect.Height}\t\t{e.ControlRect.X}:{e.ControlRect.Y}:{e.ControlRect.Width}:{e.ControlRect.Height}");
		}

		protected override void OnCellDoubleClicked(MouseCellEventArgs e)
		{
			base.OnCellDoubleClicked(e);

			Trace.WriteLine($"{e.Row}:{e.Coulmn} double click\t\t{e.GridRect.X}:{e.GridRect.Y}:{e.GridRect.Width}:{e.GridRect.Height}\t\t{e.ControlRect.X}:{e.ControlRect.Y}:{e.ControlRect.Width}:{e.ControlRect.Height}");
		}

		protected override void OnMouseDownOverCell(MouseCellEventArgs e)
		{
			base.OnMouseDownOverCell(e);

			Trace.WriteLine($"{e.Row}:{e.Coulmn} down\t\t{e.GridRect.X}:{e.GridRect.Y}:{e.GridRect.Width}:{e.GridRect.Height}\t\t{e.ControlRect.X}:{e.ControlRect.Y}:{e.ControlRect.Width}:{e.ControlRect.Height}");
		}

		protected override void OnMouseUpOverCell(MouseCellEventArgs e)
		{
			base.OnMouseUpOverCell(e);

			Trace.WriteLine($"{e.Row}:{e.Coulmn} up\t\t{e.GridRect.X}:{e.GridRect.Y}:{e.GridRect.Width}:{e.GridRect.Height}\t\t{e.ControlRect.X}:{e.ControlRect.Y}:{e.ControlRect.Width}:{e.ControlRect.Height}");
		}

		protected override void OnMouseMovedOverGrid(MouseCellEventArgs e)
		{
			base.OnMouseMovedOverGrid(e);

			Trace.WriteLine($"{e.Row}:{e.Coulmn} moved\t\t{e.GridRect.X}:{e.GridRect.Y}:{e.GridRect.Width}:{e.GridRect.Height}\t\t{e.ControlRect.X}:{e.ControlRect.Y}:{e.ControlRect.Width}:{e.ControlRect.Height}");
		}

		protected override void OnMouseEnteredGrid(EventArgs e)
		{
			base.OnMouseEnteredGrid(e);

			Trace.WriteLine($"entered");
		}

		protected override void OnMouseLeftGrid(EventArgs e)
		{
			base.OnMouseLeftGrid(e);

			Trace.WriteLine($"left");
		}
	}
}
