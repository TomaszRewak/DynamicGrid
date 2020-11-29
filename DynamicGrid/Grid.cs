using DynamicGrid.Buffers;
using DynamicGrid.Threading;
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

		private int _offsetX;
		public int OffsetX
		{
			get => _offsetX;
			set
			{
				if (_offsetX == value) return;

				var oldVisibleColumns = VisibleColumns;
				_offsetX = value;
				var newVisibleColumns = VisibleColumns;

				if (newVisibleColumns != oldVisibleColumns)
					InvalidateData();

				Invalidate();
			}
		}

		private (int MinColumn, int MaxColumn) VisibleColumns
		{
			get
			{
				var offset = 0;

				var minColumn = 0;
				while (minColumn < Columns.Length - 1 && offset + Columns[minColumn].Width < OffsetX)
					offset += Columns[minColumn++].Width;

				var maxColumn = minColumn;
				while (maxColumn < Columns.Length - 1 && offset < OffsetX + Width)
					offset += Columns[maxColumn++].Width;

				return (minColumn, maxColumn);
			}
		}

		private int ColumnsWidth
		{
			get
			{
				int width = 0;
				foreach (var column in Columns)
					width += column.Width;
				return width;
			}
		}

		public Grid()
		{
			BackColor = Color.BlueViolet;

			_cellBuffer = new CellBuffer();
			_displayBuffer = new DisplayBuffer(CreateGraphics());
		}

		private readonly Ref<bool> _invalidateDataGuard = new();
		public void InvalidateData() =>
		this.DispatchOnce(_invalidateDataGuard, () =>
		{
			if (RowSupplier == null) return;

			const int rowHeight = 20;

			_cellBuffer.Resize(Columns.Length, Height / rowHeight + 1);
			_displayBuffer.Resize(new Size(ColumnsWidth, Height));

			int minColumnOffset = int.MaxValue,
				maxColumnOffset = int.MinValue,
				minRowOffset = int.MaxValue,
				maxRowOffset = int.MinValue;

			var (minColumn, maxColumn) = VisibleColumns;
			var initialColumnOffset = GetOffset(minColumn);

			for (int rowIndex = 0, rowOffset = 0; rowOffset < Height; rowIndex++, rowOffset += rowHeight)
			{
				var row = RowSupplier.Get(rowIndex);

				for (int columnIndex = minColumn, columnOffset = initialColumnOffset; columnOffset < Width && columnIndex <= maxColumn; columnOffset += Columns[columnIndex++].Width)
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

			Trace.WriteLine($"{minColumn}:{maxColumn}");

			Invalidate(new Rectangle(
				minColumnOffset - OffsetX,
				minRowOffset,
				maxColumnOffset - minColumnOffset,
				maxRowOffset - minRowOffset));
		});

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);

			InvalidateData();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			_displayBuffer.Flush(e.ClipRectangle, OffsetX);
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{ }

		private int GetOffset(int column)
		{
			var offset = 0;
			for (var c = 0; c < column; c++)
				offset += Columns[c].Width;
			return offset;
		}
	}
}
