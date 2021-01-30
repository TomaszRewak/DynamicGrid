using DynamicGrid.Buffers;
using DynamicGrid.Data;
using DynamicGrid.Interop;
using DynamicGrid.Managers;
using DynamicGrid.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DynamicGrid
{
	/// <summary>
	/// Defines a base class for high performance grid controls.
	/// </summary>
	[System.ComponentModel.DesignerCategory("")]
	public abstract class Grid : Control
	{
		private readonly Graphics _graphics;
		private readonly IntPtr _graphicsHdc;
		private readonly CellBuffer _cellBuffer;
		private readonly DisplayBuffer _displayBuffer;
		private readonly FontManager _fontManager;

		private Rectangle _invalidDataRegion = Rectangle.Empty;
		private Point _mousePosition;
		private (int Row, int Column) _mouseCell;
		private bool _isMouseOverControl;
		private bool _isMouseOverGrid;
		private bool _isMouseDownOverGrid;

		private readonly List<ColumnPlacement> _columns = new();
		/// <summary>
		/// Gets or sets column widths. The provided collection is also being used to determine the number of columns to be displayed.
		/// </summary>
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
				ProcessMouseEvents();

				ColumnsChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		private int _horizontalOffset;
		/// <summary>
		/// Horizontal offset applied to the content of the control.
		/// It's highly recommended to use this property over placing the <c>Grid</c> in a scrollable panel. This way the grid can avoid processing and rendering of invisible columns (columns that are out of the scrolling area).
		/// </summary>
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
				ProcessMouseEvents();

				HorizontalOffsetChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		private int _verticalOffset;
		/// <summary>
		/// Vertical offset applied to the content of the control.
		/// It's highly recommended to use this property over placing the <c>Grid</c> in a scrollable panel. This way the grid can avoid processing and rendering of invisible rows (rows that are out of the scrolling area).
		/// </summary>
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
				ProcessMouseEvents();

				VerticalOffsetChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Height of a single row. This height includes a cell height and one extra pixel reserved for a bottom border.
		/// </summary>
		/// <remarks>
		/// This is a read only property. The height of a cell is derived directly from the font size and can by altered by changing it.
		/// </remarks>
		public int RowHeight => _fontManager.FontHeight + 1;

		/// <summary>
		/// A <c>Rectangle</c> representing column and row indexes visible within the current working area of the grid.
		/// </summary>
		/// <remarks>
		/// The value of this property can be affected by both the size of the control, as well as applied <see cref="HorizontalOffset"/> and <see cref="VerticalOffset"/>.
		/// </remarks>
		public Rectangle VisibleCells => new Rectangle(
			VisibleColumns.MinColumn,
			VisibleRows.MinRow,
			VisibleColumns.MaxColumn - VisibleColumns.MinColumn + 1,
			VisibleRows.MaxRow - VisibleRows.MinRow + 1);

		/// <summary>
		/// An (inclusive) range of column indexes that are visible within the current working area of the grid.
		/// </summary>
		/// <remarks>
		/// Range of visible columns is directly affected by the control width, as well as configured <see cref="Columns"/> and the applied <see cref="HorizontalOffset"/>.
		/// </remarks>
		public (int MinColumn, int MaxColumn) VisibleColumns { get; private set; }
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
			{
				_cellBuffer.ClearColumn(_cellBuffer.CropRow(c), BackColor);
				_displayBuffer.ClearColumn(_columns[c].CroppedOffset, _columns[c].Width, BackColor);
				InvalidateColumnData(c);
			}
			for (int c = newMaxColumn; c >= newMinColumn && c > oldMaxColumn; c--)
			{
				_cellBuffer.ClearColumn(_cellBuffer.CropRow(c), BackColor);
				_displayBuffer.ClearColumn(_columns[c].CroppedOffset, _columns[c].Width, BackColor);
				InvalidateColumnData(c);
			}
		}

		/// <summary>
		/// An (inclusive) range of row indexes that are visible within the current working area of the grid.
		/// </summary>
		/// <remarks>
		/// Range of visible rows is directly affected by the control height, as well as the configured <see cref="RowHeight"/> and the applied <see cref="HorizontalOffset"/>.
		/// </remarks>
		public (int MinRow, int MaxRow) VisibleRows { get; private set; }
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
			_fontManager = new FontManager();

			Font = new Font("Microsoft Sans Serif", 10);
			BackColor = Color.LightGray;

			_fontManager.Load(Font);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_graphics.Dispose();
				_displayBuffer.Dispose();
				_fontManager.Dispose();
			}

			base.Dispose(disposing);
		}

		/// <summary>
		/// A virtual method to be overwritten by a deriving class to provides the cell content and layout for requested cell coordinates.
		/// </summary>
		/// <remarks>
		/// When the data invalidation occurs, this method will be called only for cells within the <see cref="VisibleCells"/> region.
		/// </remarks>
		protected virtual Cell GetCell(int rowIndex, int columnIndex) => Cell.Empty;

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

		/// <summary>
		/// Invalidates content and layout of all cells within the <see cref="VisibleCells"/> region.
		/// </summary>
		/// <seealso cref="UpdateData"/>
		public void InvalidateData()
		{
			var (minColumn, maxColumn) = VisibleColumns;
			var (minRow, maxRow) = VisibleRows;

			InvalidateData(minRow, maxRow, minColumn, maxColumn);
		}

		/// <summary>
		/// Invalidates content and layout of all visible cells within a given row.
		/// </summary>
		/// <param name="row">Index of a row to be invalidated.</param>
		/// <seealso cref="UpdateData"/>
		public void InvalidateRowData(int row)
		{
			var (minColumn, maxColumn) = VisibleColumns;

			InvalidateData(row, row, minColumn, maxColumn);
		}

		/// <summary>
		/// Invalidates content and layout of all visible cells within a given column.
		/// </summary>
		/// <param name="column">Index of a column to be invalidated.</param>
		/// <seealso cref="UpdateData"/>
		public void InvalidateColumnData(int column)
		{
			var (minRow, maxRow) = VisibleRows;

			InvalidateData(minRow, maxRow, column, column);
		}

		/// <summary>
		/// Invalidates the content and layout of a specific cell.
		/// </summary>
		/// <param name="row">Row index of a cell to be invalidated.</param>
		/// <param name="column">Column index of a cell to be invalidated.</param>
		/// <seealso cref="UpdateData"/>
		public void InvalidateCellData(int row, int column)
		{
			InvalidateData(row, row, column, column);
		}

		/// <summary>
		/// Invalidates content and layout of all visible cells within the specified region
		/// </summary>
		/// <seealso cref="UpdateData"/>
		public void InvalidateData(int minRow, int maxRow, int minColumn, int maxColumn)
		{
			var region = new Rectangle(
				minColumn,
				minRow,
				maxColumn - minColumn + 1,
				maxRow - minRow + 1);

			_invalidDataRegion = RectangleUtils.Union(
				Rectangle.Intersect(VisibleCells, _invalidDataRegion),
				Rectangle.Intersect(VisibleCells, region));
		}

		/// <summary>
		/// Causes the grid to fetch (using the <see cref="GetCell(int, int)"/> method) the current content and layout of invalidated cells and paints those cells on an internal buffer if changed.
		/// </summary>
		/// <remarks>
		/// Execution of this method will result in an invalidation of a control region affected by the data update. The visual changed can be later flushed to the screen using the <see cref="Control.Update"/> method or by waiting for the next draw cycle.
		/// This operation clears the accumulated data invalidation region.
		/// </remarks>
		public void UpdateData()
		{
			if (IsDisposed) return;

			_invalidDataRegion = Rectangle.Intersect(VisibleCells, _invalidDataRegion);

			if (_invalidDataRegion.IsEmpty) return;
			if (_columns.Count == 0) return;

			var (minRow, maxRow) = VisibleRows;
			var (minColumn, maxColumn) = VisibleColumns;

			minColumn = Math.Max(minColumn, _invalidDataRegion.Left);
			maxColumn = Math.Min(maxColumn, _invalidDataRegion.Right - 1);
			minRow = Math.Max(minRow, _invalidDataRegion.Top);
			maxRow = Math.Min(maxRow, _invalidDataRegion.Bottom - 1);

			_invalidDataRegion = Rectangle.Empty;

			CellRenderingContext renderingContext = new CellRenderingContext();

			for (int rowIndex = minRow; rowIndex <= maxRow; rowIndex++)
				for (int columnIndex = minColumn; columnIndex <= maxColumn; columnIndex++)
					UpdateCellData(rowIndex, columnIndex, GetCell(rowIndex, columnIndex), ref renderingContext);

			Invalidate(renderingContext.InvalidatedRect);
		}

		private void UpdateCellData(int rowIndex, int columnIndex, in Cell cell, ref CellRenderingContext renderingContext)
		{
			var croppedRowIndex = _cellBuffer.CropRow(rowIndex);
			var croppedColumnIndex = _columns[columnIndex].CroppedIndex;
			var changed = _cellBuffer.TrySet(croppedRowIndex, croppedColumnIndex, in cell);

			if (!changed) return;

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

			var backgroundColor = cell.BackgroundColor ?? BackColor;
			var foregroundColor = cell.ForegroundColor ?? ForeColor;

			if (renderingContext.CurrentBackgroundColor != backgroundColor)
			{
				renderingContext.CurrentBackgroundColor = backgroundColor;
				Gdi32.SetBackgroundColor(_displayBuffer.Hdc, backgroundColor);
			}

			if (renderingContext.CurrentForegroundColor != foregroundColor)
			{
				renderingContext.CurrentForegroundColor = foregroundColor;
				Gdi32.SetForegroundColor(_displayBuffer.Hdc, foregroundColor);
			}

			if (renderingContext.CurrentAlignemnt != cell.TextAlignment)
			{
				renderingContext.CurrentAlignemnt = cell.TextAlignment;
				Gdi32.SetTextAlignemnt(_displayBuffer.Hdc, cell.TextAlignment);
			}

			if (renderingContext.CurrentFontStyle != cell.FontStyle)
			{
				renderingContext.CurrentFontStyle = cell.FontStyle;
				Gdi32.SelectObject(_displayBuffer.Hdc, _fontManager.GetHdc(cell.FontStyle));
			}

			Gdi32.PrintText(_displayBuffer.Hdc, croppedRectangle, cell.TextAlignment, cell.Text);

			renderingContext.InvalidatedRect = RectangleUtils.Union(renderingContext.InvalidatedRect, realRectangle);
		}

		/// <summary>
		/// Calls the <see cref="InvalidateData"/> and <see cref="UpdateData"/> methods in succession.
		/// </summary>
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
			if (IsDisposed) return;

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
				_columns[minColumn].CroppedOffset + destinationRect.Left + HorizontalOffset - _columns[minColumn].RealOffset,
				_cellBuffer.CropRow(minRow) * RowHeight + destinationRect.Top + VerticalOffset - minRow * RowHeight,
				destinationRect.Width,
				destinationRect.Height);
			var bufferSize = new Size(
				_columns[minColumn].CroppedRowWidth,
				_displayBuffer.Size.Height);

			if (sourceRect.Left < bufferSize.Width && sourceRect.Top < bufferSize.Height)
			{
				var source = sourceRect.Location;
				var destination = destinationRect.Location;
				var size = new Size(
					Math.Min(destinationRect.Width, bufferSize.Width - sourceRect.Left),
					Math.Min(destinationRect.Height, bufferSize.Height - sourceRect.Top));

				Gdi32.Copy(_displayBuffer.Hdc, source, _graphicsHdc, destination, size);
			}

			if (sourceRect.Right > bufferSize.Width && sourceRect.Top < bufferSize.Height)
			{
				var source = new Point(
					Math.Max(0, sourceRect.Left - bufferSize.Width),
					sourceRect.Top);
				var destination = new Point(
					destinationRect.Left + Math.Max(0, bufferSize.Width - sourceRect.Left),
					destinationRect.Top);
				var size = new Size(
					destinationRect.Width - Math.Max(0, bufferSize.Width - sourceRect.Left),
					Math.Min(destinationRect.Height, bufferSize.Height - sourceRect.Top));

				Gdi32.Copy(_displayBuffer.Hdc, source, _graphicsHdc, destination, size);
			}

			if (sourceRect.Left < bufferSize.Width && sourceRect.Bottom > bufferSize.Height)
			{
				var source = new Point(
					sourceRect.Left,
					Math.Max(0, sourceRect.Top - bufferSize.Height));
				var destination = new Point(
					destinationRect.Left,
					destinationRect.Top + Math.Max(0, bufferSize.Height - sourceRect.Top));
				var size = new Size(
					Math.Min(destinationRect.Width, bufferSize.Width - sourceRect.Left),
					destinationRect.Height - Math.Max(0, bufferSize.Height - sourceRect.Top));

				Gdi32.Copy(_displayBuffer.Hdc, source, _graphicsHdc, destination, size);
			}

			if (sourceRect.Right > bufferSize.Width && sourceRect.Bottom > bufferSize.Height)
			{
				var source = new Point(
					Math.Max(0, sourceRect.Left - bufferSize.Width),
					Math.Max(0, sourceRect.Top - bufferSize.Height));
				var destination = new Point(
					destinationRect.Left + Math.Max(0, bufferSize.Width - sourceRect.Left),
					destinationRect.Top + Math.Max(0, bufferSize.Height - sourceRect.Top));
				var size = new Size(
					destinationRect.Width - Math.Max(0, bufferSize.Width - sourceRect.Left),
					destinationRect.Height - Math.Max(0, bufferSize.Height - sourceRect.Top));

				Gdi32.Copy(_displayBuffer.Hdc, source, _graphicsHdc, destination, size);
			}
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

			UpdateVisibleRows();
			ResizeBuffers();
			RefreshData();
			Refresh();
			ProcessMouseEvents();
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);

			if (_isMouseOverGrid)
				OnCellClicked(CreateMouseCellEventArgs());
		}

		protected override void OnDoubleClick(EventArgs e)
		{
			base.OnDoubleClick(e);

			if (_isMouseOverGrid)
				OnCellDoubleClicked(CreateMouseCellEventArgs());
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (_isMouseOverGrid)
			{
				_isMouseDownOverGrid = true;
				OnMouseDownOverCell(CreateMouseCellEventArgs());
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if (_isMouseDownOverGrid)
			{
				_isMouseDownOverGrid = false;
				OnMouseUpOverCell(CreateMouseCellEventArgs());
				ProcessMouseEvents();
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			_mousePosition = e.Location;
			ProcessMouseEvents();
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);

			_isMouseOverControl = true;
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			_isMouseOverControl = false;
			ProcessMouseEvents();
		}

		private void ProcessMouseEvents()
		{
			if (!_isMouseOverControl && !_isMouseOverGrid) return;

			var x = _mousePosition.X + HorizontalOffset;
			var y = _mousePosition.Y + VerticalOffset;

			var oldMouseCell = _mouseCell;
			var oldIsMouseOverGrid = _isMouseOverGrid;

			_mouseCell = (
				y >= 0 ? y / RowHeight : (y - RowHeight + 1) / RowHeight,
				ColumnPlacement.GetColumnIndex(_columns, x, _mouseCell.Column));
			_isMouseOverGrid =
				_isMouseDownOverGrid ||
				_isMouseOverControl &&
				_columns.Count > 0 &&
				x >= 0 &&
				x < _columns[_columns.Count - 1].RealOffsetPlusWidth;

			var mouseCellChanged = _mouseCell != oldMouseCell;
			var isMouseOverGridChanged = _isMouseOverGrid != oldIsMouseOverGrid;

			switch ((_isMouseDownOverGrid, _isMouseOverGrid, isMouseOverGridChanged, mouseCellChanged))
			{
				case (true, _, _, true):
				case (false, true, false, true):
					OnMouseMovedOverGrid(CreateMouseCellEventArgs());
					break;
				case (false, true, true, _):
					OnMouseEnteredGrid(EventArgs.Empty);
					break;
				case (false, false, true, _):
					OnMouseLeftGrid(EventArgs.Empty);
					break;
			};
		}

		private MouseCellEventArgs CreateMouseCellEventArgs()
		{
			var gridRect = new Rectangle(
				_columns[_mouseCell.Column].RealOffset,
				_mouseCell.Row * RowHeight,
				_columns[_mouseCell.Column].Width,
				RowHeight);

			var controlRect = new Rectangle(
				gridRect.X - HorizontalOffset,
				gridRect.Y - VerticalOffset,
				gridRect.Width,
				gridRect.Height);

			return new MouseCellEventArgs(_mouseCell.Row, _mouseCell.Column, MouseButtons, gridRect, controlRect);
		}

		/// <summary>
		/// Raises the <see cref="CellClicked"/> event
		/// </summary>
		protected virtual void OnCellClicked(MouseCellEventArgs e) => CellClicked?.Invoke(this, e);
		/// <summary>
		/// Raises the <see cref="CellDoubleClicked"/> event
		/// </summary>
		protected virtual void OnCellDoubleClicked(MouseCellEventArgs e) => CellDoubleClicked?.Invoke(this, e);
		/// <summary>
		/// Raises the <see cref="MouseDownOverCell"/> event
		/// </summary>
		protected virtual void OnMouseDownOverCell(MouseCellEventArgs e) => MouseDownOverCell?.Invoke(this, e);
		/// <summary>
		/// Raises the <see cref="MouseUpOverCell"/> event
		/// </summary>
		protected virtual void OnMouseUpOverCell(MouseCellEventArgs e) => MouseUpOverCell?.Invoke(this, e);
		/// <summary>
		/// Raises the <see cref="MouseMovedOverGrid"/> event
		/// </summary>
		protected virtual void OnMouseMovedOverGrid(MouseCellEventArgs e) => MouseMovedOverGrid?.Invoke(this, e);
		/// <summary>
		/// Raises the <see cref="MouseEnteredGrid"/> event
		/// </summary>
		protected virtual void OnMouseEnteredGrid(EventArgs e) => MouseEnteredGrid?.Invoke(this, e);
		/// <summary>
		/// Raises the <see cref="MouseLeftGrid"/> event
		/// </summary>
		protected virtual void OnMouseLeftGrid(EventArgs e) => MouseLeftGrid?.Invoke(this, e);

		public event EventHandler<EventArgs> ColumnsChanged;
		public event EventHandler<EventArgs> HorizontalOffsetChanged;
		public event EventHandler<EventArgs> VerticalOffsetChanged;
		public event EventHandler<MouseCellEventArgs> CellClicked;
		public event EventHandler<MouseCellEventArgs> CellDoubleClicked;
		public event EventHandler<MouseCellEventArgs> MouseDownOverCell;
		public event EventHandler<MouseCellEventArgs> MouseUpOverCell;
		public event EventHandler<MouseCellEventArgs> MouseMovedOverGrid;
		public event EventHandler<EventArgs> MouseEnteredGrid;
		public event EventHandler<EventArgs> MouseLeftGrid;
	}
}
