using DynamicGrid.Buffers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynamicGrid
{
	[System.ComponentModel.DesignerCategory("")]
	public partial class Grid : UserControl
	{
		private readonly CellBuffer _cellBuffer;
		private readonly DisplayBuffer _displayBuffer;

		private IDataSupplier _dataSupplier;
		public IDataSupplier DataSupplier
		{
			get => _dataSupplier;
			set
			{
				_dataSupplier = value;

				InvalidateData();
			}
		}

		public Grid()
		{
			BackColor = Color.BlueViolet;

			_cellBuffer = new CellBuffer();
			_displayBuffer = new DisplayBuffer(CreateGraphics().GetHdc());
		}

		public void InvalidateData()
		{
			if (DataSupplier == null) return;

			const int rowHeight = 20;
			const int columnWidth = 70;

			_cellBuffer.Resize(Width / columnWidth + 1, Height / rowHeight + 1);
			_displayBuffer.Resize(Size);

			int minColumnOffset = int.MaxValue,
				maxColumnOffset = int.MinValue,
				minRowOffset = int.MaxValue,
				maxRowOffset = int.MinValue;

			for (int row = 0, rowOffset = 0; rowOffset < Height; row++, rowOffset += rowHeight)
			{
				for (int column = 0, columnOffset = 0; columnOffset < Width; column++, columnOffset += columnWidth)
				{
					var cell = DataSupplier.Get(row, column);
					var changed = _cellBuffer.TrySet(row, column, in cell);

					if (changed)
					{
						_displayBuffer.Draw(columnOffset, rowOffset, columnWidth, rowHeight, cell);

						minColumnOffset = Math.Min(minColumnOffset, columnOffset);
						maxColumnOffset = Math.Max(maxColumnOffset, columnOffset + columnWidth);
						minRowOffset = Math.Min(minRowOffset, rowOffset);
						maxRowOffset = Math.Max(maxRowOffset, rowOffset + rowHeight);
					}
				}
			}

			Invalidate(new Rectangle(
				minColumnOffset,
				minRowOffset,
				maxColumnOffset - minColumnOffset,
				maxRowOffset - minRowOffset));
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			_displayBuffer.Flush(e.ClipRectangle);
		}

		private void BeginInvoke(Action p)
		{
			base.BeginInvoke(p);
		}
	}
}
