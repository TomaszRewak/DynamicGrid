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
	public class Grid : Control
	{
		private readonly Graphics _graphics;
		private readonly IntPtr _graphicsHdc;
		private readonly CellBuffer _cellBuffer;
		private readonly DisplayBuffer _displayBuffer;
		private readonly FontManager _fontManager;
		private readonly Ref<bool> _dataInvalidationGuard = new();
		private Rectangle _invalidDataRegion = Rectangle.Empty;

		private int[] _columnWidths = Array.Empty<int>();
		public IReadOnlyCollection<int> ColumnWidths
		{
			get => _columnWidths.ToArray();
			set
			{
				_columnWidths = value.ToArray();

				ColumnsWidth = _columnWidths.Sum();

				ClearBuffers();
				InvalidateData();
				Invalidate();
			}
		}

		public int ColumnsWidth { get; set; }

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
				while (minColumn < _columnWidths.Length - 1 && offset + _columnWidths[minColumn] < HorizontalOffset)
					offset += _columnWidths[minColumn++];

				var maxColumn = minColumn;
				while (maxColumn < _columnWidths.Length - 1 && offset + _columnWidths[maxColumn] < HorizontalOffset + Width)
					offset += _columnWidths[maxColumn++];

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

		public virtual Cell GetCell(int row, int column)
		{
			return Cell.Empty;
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

			_cellBuffer.Resize(_columnWidths.Length, Height / RowHeight + 1);
			_displayBuffer.Resize(new Size(Math.Max(ColumnsWidth, Width + HorizontalOffset), Height));

			var initialColumnOffset = GetColumnOffset(minColumn);
			var initialRowOffset = GetRowOffset(minRow);

			var drawingContext = _displayBuffer.CreateDrawingContext();

			for (int rowIndex = minRow, rowOffset = initialRowOffset; rowIndex <= maxRow; rowIndex++, rowOffset += RowHeight)
			{
				for (int columnIndex = minColumn, columnOffset = initialColumnOffset; columnIndex <= maxColumn; columnOffset += _columnWidths[columnIndex++])
				{
					var cell = GetCell(rowIndex, columnIndex);
					var changed = _cellBuffer.TrySet(rowIndex, columnIndex, in cell);

					if (changed)
						drawingContext.Draw(columnOffset, rowOffset, _columnWidths[columnIndex], RowHeight, cell);
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

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
		}

		private int GetColumnOffset(int column)
		{
			var offset = 0;
			for (var c = 0; c < column; c++)
				offset += _columnWidths[c];
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

		private void ClearBuffers()
		{
			_cellBuffer.Clear(BackColor);
			_displayBuffer.Clear(BackColor);
		}

		//public event EventHandler<CellEventArgs> CellClick;
		//public event EventHandler<CellEventArgs> CellDoubleClick;
		//public event EventHandler<CellEventArgs> CellMouseDown;
		//public event EventHandler<CellEventArgs> CellMouseUp;
		//public event EventHandler<CellEventArgs> CellMouseMove;
	}
}
