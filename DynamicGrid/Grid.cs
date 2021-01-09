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
	public abstract class Grid<TRow> : Control
	{
		private readonly Graphics _graphics;
		private readonly IntPtr _graphicsHdc;
		private readonly CellBuffer _cellBuffer;
		private readonly DisplayBuffer _displayBuffer;
		private readonly FontManager _fontManager;
		private readonly Ref<bool> _dataInvalidationGuard = new();
		private Rectangle _invalidDataRegion = Rectangle.Empty;

		private IReadOnlyList<Column<TRow>> _columns = Array.Empty<Column<TRow>>();
		public IReadOnlyList<Column<TRow>> Columns
		{
			get => _columns;
			set
			{
				foreach (var column in _columns)
					column.WidthChanged -= OnColumnWidthChanged;

				_columns = value.ToList().AsReadOnly();

				foreach (var column in _columns)
					column.WidthChanged += OnColumnWidthChanged; ;

				ColumnsWidth = _columns.Sum(c => c.Width);

				InvalidateBuffers();
			}
		}

		public int ColumnsWidth { get; private set; }

		public int RowHeight => _fontManager.FontHeight + 1;

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

				for (int c = newVisibleColumns.MinColumn; c <= newVisibleColumns.MaxColumn && c < oldVisibleColumns.MinColumn; c++)
					InvalidateColumn(c);
				for (int c = newVisibleColumns.MaxColumn; c >= newVisibleColumns.MinColumn && c > oldVisibleColumns.MaxColumn; c--)
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
				while (minColumn < Columns.Count - 1 && offset + Columns[minColumn].Width < HorizontalOffset)
					offset += Columns[minColumn++].Width;

				var maxColumn = minColumn;
				while (maxColumn < Columns.Count - 1 && offset + Columns[maxColumn].Width < HorizontalOffset + Width)
					offset += Columns[maxColumn++].Width;

				return (minColumn, maxColumn);
			}
		}

		private (int MinRow, int MaxRow) VisibleRows
		{
			get => (0, Height / RowHeight);
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

		public abstract TRow GetRow(int rowIndex);
		public virtual bool ValidateRow(int rowIndex) => true;
		public virtual Cell StyleCell(int rowIndex, int columnIndex, Cell cell) => cell;

		private void InvalidateBuffers()
		{
			_cellBuffer.Clear(BackColor);
			_displayBuffer.Clear(BackColor);

			InvalidateData();
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
			var (minRow, maxRow) = VisibleRows;
			var (minColumn, maxColumn) = VisibleColumns;

			minColumn = Math.Max(minColumn, _invalidDataRegion.Left);
			maxColumn = Math.Min(maxColumn, _invalidDataRegion.Right);
			minRow = Math.Max(minRow, _invalidDataRegion.Top);
			maxRow = Math.Min(maxRow, _invalidDataRegion.Bottom);

			_cellBuffer.Resize(Columns.Count, Height / RowHeight + 1);
			_displayBuffer.Resize(new Size(Math.Max(ColumnsWidth, Width + HorizontalOffset), Height));

			var initialColumnOffset = GetColumnOffset(minColumn);
			var initialRowOffset = GetRowOffset(minRow);

			var drawingContext = _displayBuffer.CreateDrawingContext();

			for (int rowIndex = minRow, rowOffset = initialRowOffset; rowIndex <= maxRow; rowIndex++, rowOffset += RowHeight)
			{
				var hasRow = ValidateRow(rowIndex);
				var row = hasRow ? GetRow(rowIndex) : default;

				for (int columnIndex = minColumn, columnOffset = initialColumnOffset; columnIndex <= maxColumn; columnOffset += Columns[columnIndex++].Width)
				{
					var column = Columns[columnIndex];
					var cell = hasRow
						? StyleCell(rowIndex, columnIndex, column.GetCell(row))
						: Cell.Empty;
					var changed = _cellBuffer.TrySet(rowIndex, columnIndex, in cell);

					if (changed)
						drawingContext.Draw(columnOffset, rowOffset, Columns[columnIndex].Width, RowHeight, cell);
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

			InvalidateBuffers();
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);

			_fontManager.Load(Font);

			InvalidateBuffers();
		}

		private void OnColumnWidthChanged(object sender, EventArgs e)
		{
			InvalidateBuffers();
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
		}

		private int GetColumnOffset(int column)
		{
			var offset = 0;
			for (var c = 0; c < column; c++)
				offset += Columns[c].Width;
			return offset;
		}

		private int GetRowOffset(int row)
		{
			return row * RowHeight;
		}

		//private int GetColumnByOffset(int offset)
		//{

		//}

		//private int GetRowByOffset(int offset)
		//{

		//}

		//public event EventHandler<CellEventArgs> CellClick;
		//public event EventHandler<CellEventArgs> CellDoubleClick;
		//public event EventHandler<CellEventArgs> CellMouseDown;
		//public event EventHandler<CellEventArgs> CellMouseUp;
		//public event EventHandler<CellEventArgs> CellMouseMove;
	}
}
