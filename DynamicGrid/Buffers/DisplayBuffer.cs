using DynamicGrid.Interop;
using System;
using System.Drawing;

namespace DynamicGrid.Buffers
{
	internal sealed class DisplayBuffer : IDisposable
	{
		private readonly IntPtr _parentHdc;

		private BufferedGraphics _buffer;
		private IntPtr _bufferHdc;

		public IntPtr Hdc => _bufferHdc;

		public Size Capacity { get; private set; }

		private Size _size;
		public Size Size
		{
			get => _size;
			set
			{
				_size = value;

				if (value.Width <= Capacity.Width &&
					value.Width >= Capacity.Width / 4 &&
					value.Height <= Capacity.Height &&
					value.Height >= Capacity.Height / 4)
					return;

				Dispose();

				Capacity = new Size(value.Width * 2, value.Height * 2);

				_buffer = BufferedGraphicsManager.Current.Allocate(_parentHdc, new Rectangle(new Point(), Capacity));
				_bufferHdc = _buffer.Graphics.GetHdc();
			}
		}

		public DisplayBuffer(IntPtr parentHdc)
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

		public void Clear(Color color)
		{
			Gdi32.SetBackgroundColor(_bufferHdc, color);
			Gdi32.Fill(_bufferHdc, new Rectangle(Point.Empty, Size));
		}

		public void ClearColumn(int offset, int width, Color color)
		{
			Gdi32.SetBackgroundColor(_bufferHdc, color);
			Gdi32.Fill(_bufferHdc, new Rectangle(offset, 0, width, Size.Height));
		}
	}
}
