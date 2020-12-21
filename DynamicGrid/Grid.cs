using DynamicGrid.Buffers;
using DynamicGrid.Managers;
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
	public class Grid<TRow> : Control
	{
		private readonly Graphics _graphics;
		private readonly IntPtr _graphicsHdc;
		private readonly CellBuffer _cellBuffer;
		private readonly DisplayBuffer _displayBuffer;
		private readonly FontManager _fontManager;
		private readonly Ref<bool> _dataInvalidationGuard = new();
		private Rectangle _invalidDataRegion = Rectangle.Empty;

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

		public int RowHeight => _fontManager.FontHeight + 1;

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

				for (int c = newVisibleColumns.MinColumn; c <= newVisibleColumns.MinColumn && c < oldVisibleColumns.MinColumn; c++)
					InvalidateColumn(c);
				for (int c = newVisibleColumns.MaxColumn; c >= newVisibleColumns.MaxColumn && c > oldVisibleColumns.MaxColumn; c--)
					InvalidateColumn(c);

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

		private (int MinRow, int MaxRow) VisibleRows
		{
			get => (0, Height / RowHeight);
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
			_graphics = CreateGraphics();
			_graphicsHdc = _graphics.GetHdc();
			_cellBuffer = new CellBuffer();
			_displayBuffer = new DisplayBuffer(_graphicsHdc);
			_fontManager = new FontManager(_graphicsHdc);

			BackColor = Color.LightGray;

			_fontManager.Load(Font);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Columns = Array.Empty<Column<TRow>>();
			}

			base.Dispose(disposing);
		}

		public void InvalidateData()
		{
			var (minColumn, maxColumn) = VisibleColumns;
			var (minRow, maxRow) = VisibleRows;

			InvalidateData(minRow, maxRow, minColumn, maxColumn);
		}

		public void InvalidateRow(int row)
		{
			var (minColumn, maxColumn) = VisibleColumns;

			InvalidateData(row, row, minColumn, maxColumn);
		}

		public void InvalidateColumn(int column)
		{
			var (minRow, maxRow) = VisibleRows;

			InvalidateData(minRow, maxRow, column, column);
		}

		public void InvalidateCell(int row, int column)
		{
			InvalidateData(row, row, column, column);
		}

		public void InvalidateData(int minRow, int maxRow, int minColumn, int maxColumn)
		{
			var region = new Rectangle(
				minColumn,
				minRow,
				maxColumn - minColumn,
				maxRow - minRow);

			_invalidDataRegion = _invalidDataRegion == Rectangle.Empty
				? region
				: Rectangle.Union(_invalidDataRegion, region);

			RefreshData();
		}

		private void RefreshData() =>
		this.DispatchOnce(_dataInvalidationGuard, () =>
		{
			if (RowSupplier == null) return;

			var (minRow, maxRow) = VisibleRows;
			var (minColumn, maxColumn) = VisibleColumns;

			minColumn = Math.Max(minColumn, _invalidDataRegion.Left);
			maxColumn = Math.Min(maxColumn, _invalidDataRegion.Right);
			minRow = Math.Max(minRow, _invalidDataRegion.Top);
			maxRow = Math.Min(maxRow, _invalidDataRegion.Bottom);

			_cellBuffer.Resize(_columns.Length, Height / RowHeight + 1);
			_displayBuffer.Resize(new Size(Math.Max(ColumnsWidth, Width + HorizontalOffset), Height));

			var initialColumnOffset = GetColumnOffset(minColumn);
			var initialRowOffset = GetRowOffset(minRow);

			var drawingContext = _displayBuffer.CreateDrawingContext();

			for (int rowIndex = minRow, rowOffset = initialRowOffset; rowIndex <= maxRow; rowIndex++, rowOffset += RowHeight)
			{
				var row = RowSupplier.Get(rowIndex);

				for (int columnIndex = minColumn, columnOffset = initialColumnOffset; columnIndex <= maxColumn; columnOffset += _columns[columnIndex++].Width)
				{
					var column = _columns[columnIndex];
					var cell = column.GetCell(row);
					var changed = _cellBuffer.TrySet(rowIndex, columnIndex, in cell);

					if (changed)
						drawingContext.Draw(columnOffset, rowOffset, column.Width, RowHeight, cell);
				}
			}

			_invalidDataRegion = Rectangle.Empty;

			var invalidatedRect = drawingContext.InvalidatedRect;
			invalidatedRect.Offset(-HorizontalOffset, 0);
			Invalidate(invalidatedRect);

			Trace.WriteLine($"{minColumn}:{maxColumn} {minRow}:{maxRow} {invalidatedRect}");
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

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);

			_fontManager.Load(Font);

			ClearBuffers();
			InvalidateData();
			Invalidate();
		}

		private void OnColumnWidthChanged(object sender, EventArgs e)
		{
			ClearBuffers();
			InvalidateData();
			Invalidate();
		}

		private int GetColumnOffset(int column)
		{
			var offset = 0;
			for (var c = 0; c < column; c++)
				offset += _columns[c].Width;
			return offset;
		}

		private int GetRowOffset(int row)
		{
			return row * RowHeight;
		}

		private void ClearBuffers()
		{
			_cellBuffer.Clear(BackColor);
			_displayBuffer.Clear(BackColor);
		}
	}
}
