using DynamicGrid.Buffers;
using DynamicGrid.Data;
using DynamicGrid.Interop;
using DynamicGrid.Managers;
using DynamicGrid.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DynamicGrid
{
	[System.ComponentModel.DesignerCategory("")]
	public abstract class Grid : Control
	{
		private readonly Graphics _graphics;
		private readonly IntPtr _graphicsHdc;
		private readonly CellBuffer _cellBuffer;
		private readonly DisplayBuffer _displayBuffer;
		private readonly FontManager _fontManager;
		private Rectangle _invalidDataRegion = Rectangle.Empty;

		private readonly List<ColumnPlacement> _columns = new();
		public IEnumerable<int> Columns
		{
			get => _columns.Select(c => c.Width);
			set
			{
				ColumnPlacement.CalculatePlacement(value, Width, _columns);

				UpdateVisibleColumns();
				ResizeBuffers();
				RefreshData();
				Refresh();
			}
		}

		private int _horizontalOffset;
		public int HorizontalOffset
		{
			get => _horizontalOffset;
			set
			{
				if (_horizontalOffset == value) return;
				_horizontalOffset = value;

				UpdateVisibleColumns();
				UpdateData();
				Refresh();
			}
		}

		private int _verticalOffset;
		public int VerticalOffset
		{
			get => _verticalOffset;
			set
			{
				if (_verticalOffset == value) return;
				_verticalOffset = value;

				UpdateVisibleRows();
				UpdateData();
				Refresh();
			}
		}

		public int RowHeight => _fontManager.FontHeight + 1;
		private Rectangle VisibleCells => new Rectangle(
			VisibleColumns.MinColumn,
			VisibleRows.MinRow,
			VisibleColumns.MaxColumn - VisibleColumns.MinColumn + 1,
			VisibleRows.MaxRow - VisibleRows.MinRow + 1);

		private (int MinColumn, int MaxColumn) VisibleColumns { get; set; }
		private void UpdateVisibleColumns()
		{
			var oldMinColumn = VisibleColumns.MinColumn;
			var newMinColumn = 0;
			while (newMinColumn < _columns.Count - 1 && _columns[newMinColumn].RealOffsetPlusWidth <= HorizontalOffset)
				newMinColumn++;

			var oldMaxColumn = VisibleColumns.MaxColumn;
			var newMaxColumn = newMinColumn;
			while (newMaxColumn < _columns.Count - 1 && _columns[newMaxColumn].RealOffsetPlusWidth < HorizontalOffset + Width)
				newMaxColumn++;

			VisibleColumns = (newMinColumn, newMaxColumn);

			for (int c = newMinColumn; c <= newMaxColumn && c < oldMinColumn; c++)
				InvalidateColumnData(c);
			for (int c = newMaxColumn; c >= newMinColumn && c > oldMaxColumn; c--)
				InvalidateColumnData(c);
		}

		private (int MinRow, int MaxRow) VisibleRows { get; set; }
		private void UpdateVisibleRows()
		{
			var oldMinRow = VisibleRows.MinRow;
			var newMinRow = VerticalOffset >= 0
				? VerticalOffset / RowHeight
				: (VerticalOffset + 1) / RowHeight - 1;

			var oldMaxRow = VisibleRows.MaxRow;
			var newMaxRow = VerticalOffset + Height > 0
				? (VerticalOffset + Height - 1) / RowHeight
				: (VerticalOffset + Height) / RowHeight - 1;

			VisibleRows = (newMinRow, newMaxRow);

			for (int r = newMinRow; r <= newMaxRow && r < oldMinRow; r++)
				InvalidateRowData(r);
			for (int r = newMaxRow; r >= newMinRow && r > oldMaxRow; r--)
				InvalidateRowData(r);
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

		public virtual Cell GetCell(int rowIndex, int columnIndex) => Cell.Empty;

		private void ResizeBuffers()
		{
			if (!_columns.Any()) return;

			var rows = Height / RowHeight + 2;

			_cellBuffer.Size = new Size(
				_columns.Max(p => p.CroppedIndex + 1),
				rows);
			_displayBuffer.Size = new Size(
				_columns.Max(p => p.CroppedOffset + p.Width),
				rows * RowHeight);

			InvalidateBuffers();
		}

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

		public void InvalidateRowData(int row)
		{
			var (minColumn, maxColumn) = VisibleColumns;

			InvalidateData(row, row, minColumn, maxColumn);
		}

		public void InvalidateColumnData(int column)
		{
			var (minRow, maxRow) = VisibleRows;

			InvalidateData(minRow, maxRow, column, column);
		}

		public void InvalidateCellData(int row, int column)
		{
			InvalidateData(row, row, column, column);
		}

		public void InvalidateData(int minRow, int maxRow, int minColumn, int maxColumn)
		{
			var region = new Rectangle(
				minColumn,
				minRow,
				maxColumn - minColumn + 1,
				maxRow - minRow + 1);

			_invalidDataRegion = Rectangle.Intersect(VisibleCells, RectangleUtils.Union(_invalidDataRegion, region));
		}

		public void UpdateData()
		{
			_invalidDataRegion = Rectangle.Intersect(VisibleCells, _invalidDataRegion);

			if (_invalidDataRegion.IsEmpty) return;
			if (_cellBuffer.Size.IsEmpty) return;

			var (minRow, maxRow) = VisibleRows;
			var (minColumn, maxColumn) = VisibleColumns;

			minColumn = Math.Max(minColumn, _invalidDataRegion.Left);
			maxColumn = Math.Min(maxColumn, _invalidDataRegion.Right - 1);
			minRow = Math.Max(minRow, _invalidDataRegion.Top);
			maxRow = Math.Min(maxRow, _invalidDataRegion.Bottom - 1);

			var currentColor = null as Color?;
			var currentAlignemnt = null as HorizontalAlignment?;
			var invalidatedRect = Rectangle.Empty;

			for (int rowIndex = minRow; rowIndex <= maxRow; rowIndex++)
			{
				for (int columnIndex = minColumn; columnIndex <= maxColumn; columnIndex++)
				{
					var cell = GetCell(rowIndex, columnIndex);

					var croppedRowIndex = _cellBuffer.CropRow(rowIndex);
					var croppedColumnIndex = _columns[columnIndex].CroppedIndex;
					var changed = _cellBuffer.TrySet(croppedRowIndex, croppedColumnIndex, in cell);

					if (!changed) continue;

					var size = new Size(
						_columns[columnIndex].Width - 1,
						RowHeight - 1);
					var realPosition = new Point(
						_columns[columnIndex].RealOffset - HorizontalOffset,
						RowHeight * rowIndex - VerticalOffset);
					var croppedPosition = new Point(
						_columns[columnIndex].CroppedOffset,
						RowHeight * croppedRowIndex);
					var realRectangle = new Rectangle(realPosition, size);
					var croppedRectangle = new Rectangle(croppedPosition, size);

					var textPosition = new Point(size.Width / 2, 0);
					var cellColor = cell.Color.A == byte.MaxValue
						? cell.Color
						: BackColor;

					if (currentColor != cellColor)
					{
						currentColor = cellColor;
						Gdi32.SetBackgroundColor(_displayBuffer.Hdc, cellColor);
					}

					if (currentAlignemnt != cell.Alignment)
					{
						currentAlignemnt = cell.Alignment;
						Gdi32.SetTextAlignemnt(_displayBuffer.Hdc, cell.Alignment);
					}

					Gdi32.PrintText(_displayBuffer.Hdc, croppedRectangle, textPosition, cell.Text);

					invalidatedRect = RectangleUtils.Union(invalidatedRect, realRectangle);
				}
			}

			_invalidDataRegion = Rectangle.Empty;

			Invalidate(invalidatedRect);

			Trace.WriteLine($"{minColumn}:{maxColumn} {minRow}:{maxRow} {invalidatedRect}");
		}

		public void RefreshData()
		{
			InvalidateData();
			UpdateData();
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);

			UpdateVisibleRows();
			Columns = Columns;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			var destinationRect = e.ClipRectangle;

			var leftEdge = 0;
			var rightEdge = _columns.Count == 0
				? 0
				: _columns[_columns.Count - 1].RealOffsetPlusWidth;

			if (destinationRect.Left + HorizontalOffset < leftEdge)
			{
				var destination = destinationRect.Location;
				var size = new Size(
					Math.Min(destinationRect.Width, leftEdge - destinationRect.Left - HorizontalOffset),
					destinationRect.Height);

				Gdi32.SetBackgroundColor(_graphicsHdc, BackColor);
				Gdi32.Fill(_graphicsHdc, new Rectangle(destination, size));

				destinationRect = new Rectangle(
					destinationRect.Left + size.Width,
					destinationRect.Top,
					destinationRect.Width - size.Width,
					destinationRect.Height);
			}

			if (destinationRect.Right + HorizontalOffset >= rightEdge)
			{
				var destination = new Point(
					Math.Max(destinationRect.Left, rightEdge - HorizontalOffset),
					destinationRect.Top);
				var size = new Size(
					destinationRect.Width - Math.Max(0, rightEdge - HorizontalOffset - destinationRect.Left),
					destinationRect.Height);

				Gdi32.SetBackgroundColor(_graphicsHdc, BackColor);
				Gdi32.Fill(_graphicsHdc, new Rectangle(destination, size));

				destinationRect = new Rectangle(
					destinationRect.Left,
					destinationRect.Top,
					destinationRect.Width - size.Width,
					destinationRect.Height);
			}

			if (_columns.Count == 0)
				return;

			var minRow = VisibleRows.MinRow;
			var minColumn = VisibleColumns.MinColumn;

			var sourceRect = new Rectangle(
				_columns[minColumn].CroppedOffset + destinationRect.X + HorizontalOffset - _columns[minColumn].RealOffset,
				_cellBuffer.CropRow(minRow) * RowHeight + destinationRect.Y + VerticalOffset - minRow * RowHeight,
				destinationRect.Width,
				destinationRect.Height);

			if (sourceRect.Left < _displayBuffer.Size.Width && sourceRect.Top < _displayBuffer.Size.Height)
			{
				var source = sourceRect.Location;
				var destination = destinationRect.Location;
				var size = new Size(
					Math.Min(destinationRect.Width, _displayBuffer.Size.Width - sourceRect.Left),
					Math.Min(destinationRect.Height, _displayBuffer.Size.Height - sourceRect.Top));

				Gdi32.Copy(_displayBuffer.Hdc, source, _graphicsHdc, destination, size);
			}

			if (sourceRect.Right > _displayBuffer.Size.Width && sourceRect.Top < _displayBuffer.Size.Height)
			{
				var source = new Point(
					Math.Max(0, sourceRect.Left - _displayBuffer.Size.Width),
					sourceRect.Top);
				var destination = new Point(
					destinationRect.Left + Math.Max(0, _displayBuffer.Size.Width - sourceRect.Left),
					destinationRect.Top);
				var size = new Size(
					destinationRect.Width - Math.Max(0, _displayBuffer.Size.Width - sourceRect.Left),
					Math.Min(destinationRect.Height, _displayBuffer.Size.Height - sourceRect.Top));

				Gdi32.Copy(_displayBuffer.Hdc, source, _graphicsHdc, destination, size);
			}

			if (sourceRect.Left < _displayBuffer.Size.Width && sourceRect.Bottom > _displayBuffer.Size.Height)
			{
				var source = new Point(
					sourceRect.Left,
					Math.Max(0, sourceRect.Top - _displayBuffer.Size.Height));
				var destination = new Point(
					destinationRect.Left,
					destinationRect.Top + Math.Max(0, _displayBuffer.Size.Height - sourceRect.Top));
				var size = new Size(
					Math.Min(destinationRect.Width, _displayBuffer.Size.Width - sourceRect.Left),
					destinationRect.Height - Math.Max(0, _displayBuffer.Size.Height - sourceRect.Top));

				Gdi32.Copy(_displayBuffer.Hdc, source, _graphicsHdc, destination, size);
			}

			if (sourceRect.Right > _displayBuffer.Size.Width && sourceRect.Bottom > _displayBuffer.Size.Height)
			{
				var source = new Point(
					Math.Max(0, sourceRect.Left - _displayBuffer.Size.Width),
					Math.Max(0, sourceRect.Top - _displayBuffer.Size.Height));
				var destination = new Point(
					destinationRect.Left + Math.Max(0, _displayBuffer.Size.Width - sourceRect.Left),
					destinationRect.Top + Math.Max(0, _displayBuffer.Size.Height - sourceRect.Top));
				var size = new Size(
					destinationRect.Width - Math.Max(0, _displayBuffer.Size.Width - sourceRect.Left),
					destinationRect.Height - Math.Max(0, _displayBuffer.Size.Height - sourceRect.Top));

				Gdi32.Copy(_displayBuffer.Hdc, source, _graphicsHdc, destination, size);
			}
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			//base.OnPaintBackground(e); // TODO remove the base call
		}

		protected override void OnBackColorChanged(EventArgs e)
		{
			base.OnBackColorChanged(e);

			InvalidateBuffers();
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);

			_fontManager.Load(Font);

			UpdateVisibleRows();
			ResizeBuffers();
			RefreshData();
			Refresh();
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
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
