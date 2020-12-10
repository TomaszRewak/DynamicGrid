using DynamicGrid.Buffers;
using DynamicGrid.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DynamicGrid
{
	[System.ComponentModel.DesignerCategory("")]
	public class Grid<TRow> : UserControl
	{
		private readonly CellBuffer _cellBuffer;
		private readonly DisplayBuffer _displayBuffer;

		private Column<TRow>[] _columns = Array.Empty<Column<TRow>>();
		public IReadOnlyCollection<Column<TRow>> Columns
		{
			get => _columns.ToArray();
			set
			{
				foreach (var column in _columns)
					column.WidthChanged -= OnColumnWidthChanged;

				_columns = value.ToArray();

				foreach (var column in _columns)
					column.WidthChanged += OnColumnWidthChanged;

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

		private int _horizontalOffset;
		public int HorizontalOffset
		{
			get => _horizontalOffset;
			set
			{
				if (_horizontalOffset == value) return;

				var oldVisibleColumns = VisibleColumns;
				_horizontalOffset = value;
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
				while (minColumn < _columns.Length - 1 && offset + _columns[minColumn].Width < HorizontalOffset)
					offset += _columns[minColumn++].Width;

				var maxColumn = minColumn;
				while (maxColumn < _columns.Length - 1 && offset + _columns[maxColumn].Width < HorizontalOffset + Width)
					offset += _columns[maxColumn++].Width;

				return (minColumn, maxColumn);
			}
		}

		private int ColumnsWidth
		{
			get
			{
				int width = 0;
				foreach (var column in _columns)
					width += column.Width;
				return width;
			}
		}

		public Grid()
		{
			_cellBuffer = new CellBuffer();
			_displayBuffer = new DisplayBuffer(CreateGraphics());

			BackColor = Color.DodgerBlue;
		}

		private readonly Ref<bool> _invalidateDataGuard = new();
		public void InvalidateData() =>
		this.DispatchOnce(_invalidateDataGuard, () =>
		{
			if (RowSupplier == null) return;

			const int rowHeight = 20;

			_cellBuffer.Resize(_columns.Length, Height / rowHeight + 1);
			_displayBuffer.Resize(new Size(Math.Max(ColumnsWidth, Width + HorizontalOffset), Height));

			int minColumnOffset = int.MaxValue,
				maxColumnOffset = int.MinValue,
				minRowOffset = int.MaxValue,
				maxRowOffset = int.MinValue;

			var (minColumn, maxColumn) = VisibleColumns;
			var initialColumnOffset = GetOffset(minColumn);
			var drawingContext = _displayBuffer.CreateDrawingContext();

			for (int rowIndex = 0, rowOffset = 0; rowOffset < Height; rowIndex++, rowOffset += rowHeight)
			{
				var row = RowSupplier.Get(rowIndex);

				for (int columnIndex = minColumn, columnOffset = initialColumnOffset; columnIndex <= maxColumn; columnOffset += _columns[columnIndex++].Width)
				{
					var column = _columns[columnIndex];
					var cell = column.GetCell(row);
					var changed = _cellBuffer.TrySet(rowIndex, columnIndex, in cell);

					if (changed)
					{
						drawingContext.Draw(columnOffset, rowOffset, column.Width, rowHeight, cell);

						minColumnOffset = Math.Min(minColumnOffset, columnOffset);
						maxColumnOffset = Math.Max(maxColumnOffset, columnOffset + column.Width);
						minRowOffset = Math.Min(minRowOffset, rowOffset);
						maxRowOffset = Math.Max(maxRowOffset, rowOffset + rowHeight);
					}
				}
			}

			var invalidatedRect = drawingContext.InvalidatedRect;
			invalidatedRect.Offset(-HorizontalOffset, 0);
			Invalidate(invalidatedRect);

			Trace.WriteLine($"{minColumn}:{maxColumn} {invalidatedRect}");
		});

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);

			InvalidateData();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			_displayBuffer.Flush(e.ClipRectangle, HorizontalOffset);
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{ }

		protected override void OnBackColorChanged(EventArgs e)
		{
			base.OnBackColorChanged(e);

			ClearBuffers();
			Invalidate();
		}

		private void OnColumnWidthChanged(object sender, EventArgs e)
		{
			ClearBuffers();
			InvalidateData();
			Invalidate();
		}

		private int GetOffset(int column)
		{
			var offset = 0;
			for (var c = 0; c < column; c++)
				offset += _columns[c].Width;
			return offset;
		}

		private void ClearBuffers()
		{
			_cellBuffer.Clear(BackColor);
			_displayBuffer.Clear(BackColor);
		}
	}
}
