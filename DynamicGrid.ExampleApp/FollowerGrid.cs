using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DynamicGrid.ExampleApp
{
	internal sealed class FollowerGrid : Grid
	{
		private readonly Random _random = new Random();

		private int _targetRow;
		public int TargetRow
		{
			get => _targetRow;
			set
			{
				if (_targetRow == value) return;
				InvalidateCellData(_targetRow, 0);
				_targetRow = value;
				InvalidateCellData(_targetRow, 0);
				UpdateData();
			}
		}

		public FollowerGrid()
		{
			Columns = new[] { 100, 100 };

			_ = new DispatcherTimer(
				TimeSpan.FromMilliseconds(1),
				DispatcherPriority.Background,
				(e, a) => Step(),
				Dispatcher.CurrentDispatcher);

			_ = new DispatcherTimer(
				TimeSpan.FromSeconds(2),
				DispatcherPriority.Background,
				(e, a) => RandomizeTargetRow(),
				Dispatcher.CurrentDispatcher);
		}

		protected override Cell GetCell(int rowIndex, int columnIndex)
		{
			return columnIndex switch
			{
				0 when rowIndex == _targetRow => new Cell(rowIndex.ToString(), Color.Gold),
				0 when rowIndex % 2 == 0 => new Cell(rowIndex.ToString(), Color.White),
				0 => new Cell(rowIndex.ToString(), Color.LightGray),
				1 => new Cell(DateTime.Now.ToLongTimeString(), Color.Gray),
				_ => throw new NotImplementedException(),
			};
		}

		private void Step()
		{
			var _target = _targetRow * RowHeight - Height / 2 + RowHeight / 2;

			if (VerticalOffset < _target)
				VerticalOffset = Math.Min(_target, VerticalOffset + 4);
			if (VerticalOffset > _target)
				VerticalOffset = Math.Max(_target, VerticalOffset - 4);
		}

		private void RandomizeTargetRow()
		{
			int maxStep = Height / RowHeight / 2;

			TargetRow += _random.Next(-maxStep, +maxStep);
		}
	}
}
