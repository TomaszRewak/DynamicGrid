using DynamicGrid.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynamicGrid.Buffers
{
	internal sealed class GridBuffer : IDisposable
	{
		private readonly IntPtr _parentHdc;

		private BufferedGraphics _buffer;
		private IntPtr _bufferHdc;
		private Size _bufferSize;
		private Color _backgroundColor;
		private Cell[,] _cells = new Cell[0, 0];

		private int[] _columnWidths;
		public int[] ColumnWidths
		{
			get => _columnWidths;
			set
			{
				if (_columnWidths == value) return;
				_columnWidths = value;

				UpdateColumnOffsets();
			}
		}

		private List<(int CellOffset, int DisplayOffset)> ColumnOffsets { get; } = new();

		private int _rowHeight;
		public int RowHeight
		{
			get => _rowHeight;
			set
			{
				if (_rowHeight == value) return;
				_rowHeight = value;
			}
		}

		private Size _parentSize;
		public Size ParentSize
		{
			get => _parentSize;
			set
			{
				if (_parentSize == value) return;
				_parentSize = value;
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
			}
		}

		private int VisibleRows { get; private set; }

		public GridBuffer(IntPtr parentHdc)
		{
			_parentHdc = parentHdc;
		}

		public void Dispose()
		{
			if (_buffer == null) return;

			_buffer.Graphics.ReleaseHdc(_bufferHdc);
			_buffer.Dispose();

			_buffer = null;
			_bufferHdc = IntPtr.Zero;
		}

		public DrawingContext CreateDrawingContext()
		{
			return new DrawingContext(_bufferHdc, _backgroundColor);
		}

		public void Clear(Color color)
		{
			_backgroundColor = color;

			Gdi32.SetBackgroundColor(_bufferHdc, color);
			Gdi32.Fill(_bufferHdc, new Rectangle(Point.Empty, _bufferSize));
		}

		public void Resize<TRow>(Size size, IEnumerable<Column<TRow>> columns, int rowHeight)
		{
			if (size.Width <= _bufferSize.Width && size.Height <= _bufferSize.Height)
				return;

			var newBufferSize = new Size(
				Math.Max(size.Width * 2, _bufferSize.Width),
				Math.Max(size.Height * 2, _bufferSize.Height));
			var newBuffer = BufferedGraphicsManager.Current.Allocate(_parentHdc, new Rectangle(new Point(), newBufferSize));
			var newBufferHdc = newBuffer.Graphics.GetHdc();

			Gdi32.SetBackgroundColor(newBufferHdc, _backgroundColor);
			Gdi32.Fill(newBufferHdc, new Rectangle(Point.Empty, newBufferSize));

			if (_buffer != null)
				Gdi32.Copy(_bufferHdc, Point.Empty, newBufferHdc, Point.Empty, _bufferSize);

			Dispose();

			_buffer = newBuffer;
			_bufferHdc = newBufferHdc;
			_bufferSize = newBufferSize;
		}

		public void Flush(Rectangle rectangle, int offsetX)
		{
			if (_buffer != null)
				Gdi32.Copy(_bufferHdc, new Point(rectangle.X + offsetX, rectangle.Y), _parentHdc, rectangle.Location, rectangle.Size);
		}
	}
}
