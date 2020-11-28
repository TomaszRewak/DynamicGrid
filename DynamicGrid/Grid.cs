using DynamicGrid.Buffers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynamicGrid
{
	[System.ComponentModel.DesignerCategory("")]
	public partial class Grid<TRow> : UserControl
	{
		private readonly CellBuffer _cellBuffer;
		private readonly DisplayBuffer _displayBuffer;

		private Column<TRow>[] _columns = Array.Empty<Column<TRow>>();
		public Column<TRow>[] Columns
		{
			get => _columns;
			set
			{
				_columns = value.ToArray();
				InvalidateData();
			}
		}

		private IRowSupplier<TRow> _rowSupplier;
		public IRowSupplier<TRow> RowSupplier
		{
			get => _rowSupplier;
			set
			{
				_rowSupplier = value;
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
			if (RowSupplier == null) return;

			const int rowHeight = 20;

			_cellBuffer.Resize(Columns.Length, Height / rowHeight + 1);
			_displayBuffer.Resize(Size);

			int minColumnOffset = int.MaxValue,
				maxColumnOffset = int.MinValue,
				minRowOffset = int.MaxValue,
				maxRowOffset = int.MinValue;

			for (int rowIndex = 0, rowOffset = 0; rowOffset < Height; rowIndex++, rowOffset += rowHeight)
			{
				var row = RowSupplier.Get(rowIndex);

				for (int columnIndex = 0, columnOffset = 0; columnOffset < Width && columnIndex < Columns.Length; columnOffset += Columns[columnIndex++].Width)
				{
					var column = Columns[columnIndex];
					var cell = column.GetCell(row);
					var changed = _cellBuffer.TrySet(rowIndex, columnIndex, in cell);

					if (changed)
					{
						_displayBuffer.Draw(columnOffset, rowOffset, column.Width, rowHeight, cell);

						minColumnOffset = Math.Min(minColumnOffset, columnOffset);
						maxColumnOffset = Math.Max(maxColumnOffset, columnOffset + column.Width);
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

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);

			InvalidateData();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			_displayBuffer.Flush(e.ClipRectangle);
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{ }
	}
}
