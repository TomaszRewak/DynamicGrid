using DynamicGrid.Buffers;
using DynamicGrid.Data;
using DynamicGrid.Interop;
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
	public abstract class Grid : Control
	{
		private readonly Graphics _graphics;
		private readonly IntPtr _graphicsHdc;
		private readonly CellBuffer _cellBuffer;
		private readonly DisplayBuffer _displayBuffer;
		private readonly FontManager _fontManager;
		private readonly Ref<bool> _dataInvalidationGuard = new();
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
				_horizontalOffset = value;

				UpdateVisibleColumns();
				Invalidate();
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
				Invalidate();
			}
		}

		public int RowHeight => _fontManager.FontHeight + 1;

		private (int MinColumn, int MaxColumn) VisibleColumns { get; set; }
		private void UpdateVisibleColumns()
		{
			var minColumn = 0;
			while (minColumn < _columns.Count - 1 && _columns[minColumn].RealOffset + _columns[minColumn].Width <= HorizontalOffset)
				minColumn++;

			var maxColumn = minColumn;
			while (maxColumn < _columns.Count - 1 && _columns[maxColumn].RealOffset + _columns[maxColumn].Width < HorizontalOffset + Width)
				maxColumn++;

			for (int c = minColumn; c <= maxColumn && c < VisibleColumns.MinColumn; c++)
				InvalidateColumn(c);
			for (int c = maxColumn; c >= minColumn && c > VisibleColumns.MaxColumn; c--)
				InvalidateColumn(c);

			VisibleColumns = (minColumn, maxColumn);
		}

		private (int MinRow, int MaxRow) VisibleRows { get; set; }
		private void UpdateVisibleRows()
		{
			var minRow = VerticalOffset >= 0
				? VerticalOffset / RowHeight
				: (VerticalOffset + 1) / RowHeight - 1;
			var maxRow = VerticalOffset + Height > 0
				? (VerticalOffset + Height - 1) / RowHeight
				: (VerticalOffset + Height) / RowHeight - 1;

			for (int r = minRow; r <= maxRow && r < VisibleRows.MinRow; r++)
				InvalidateRow(r);
			for (int r = maxRow; r >= minRow && r > VisibleRows.MaxRow; r--)
				InvalidateRow(r);

			VisibleRows = (minRow, maxRow);
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

					invalidatedRect = invalidatedRect == Rectangle.Empty
						? realRectangle
						: Rectangle.Union(invalidatedRect, realRectangle);
				}
			}

			_invalidDataRegion = Rectangle.Empty;

			Invalidate(invalidatedRect);

			Trace.WriteLine($"{minColumn}:{maxColumn} {minRow}:{maxRow} {invalidatedRect}");
		});

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);

			UpdateVisibleRows();
			Columns = Columns;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			var destinationRect = e.ClipRectangle;

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
			base.OnPaintBackground(e); // TODO remove the base call
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
			InvalidateData();
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
