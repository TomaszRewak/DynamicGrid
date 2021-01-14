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
	internal sealed class DisplayBuffer : IDisposable
	{
		private readonly IntPtr _parentHdc;

		private BufferedGraphics _buffer;
		private IntPtr _bufferHdc;
		private Size _bufferSize;
		private Color _backgroundColor;

		public IntPtr Hdc => _bufferHdc;

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
			_backgroundColor = color;

			Gdi32.SetBackgroundColor(_bufferHdc, color);
			Gdi32.Fill(_bufferHdc, new Rectangle(Point.Empty, _bufferSize));
		}

		public void Grow(int width, int height)
		{
			if (width <= _bufferSize.Width && height <= _bufferSize.Height)
				return;

			Dispose();

			_bufferSize = new Size(
				Math.Max(width * 2, _bufferSize.Width),
				Math.Max(height * 2, _bufferSize.Height));
			_buffer = BufferedGraphicsManager.Current.Allocate(_parentHdc, new Rectangle(new Point(), _bufferSize));
			_bufferHdc = _buffer.Graphics.GetHdc();

			Gdi32.SetBackgroundColor(_bufferHdc, _backgroundColor);
			Gdi32.Fill(_bufferHdc, new Rectangle(Point.Empty, _bufferSize));
		}
	}
}
