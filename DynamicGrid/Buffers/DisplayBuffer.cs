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

		private Size _size;
		public Size Size
		{
			get => _size;
			set
			{
				_size = value;

				if (value.Width <= _bufferSize.Width && value.Height <= _bufferSize.Height)
					return;

				Dispose();

				_bufferSize = new Size(
					Math.Max(value.Width * 2, _bufferSize.Width),
					Math.Max(value.Height * 2, _bufferSize.Height));
				_buffer = BufferedGraphicsManager.Current.Allocate(_parentHdc, new Rectangle(new Point(), _bufferSize));
				_bufferHdc = _buffer.Graphics.GetHdc();

				Gdi32.SetBackgroundColor(_bufferHdc, _backgroundColor);
				Gdi32.Fill(_bufferHdc, new Rectangle(Point.Empty, _bufferSize));
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
			_backgroundColor = color;

			Gdi32.SetBackgroundColor(_bufferHdc, color);
			Gdi32.Fill(_bufferHdc, new Rectangle(Point.Empty, _bufferSize));
		}

		public void ClearColumn(int offset, int width, Color color)
		{
			_backgroundColor = color;

			Gdi32.SetBackgroundColor(_bufferHdc, color);
			Gdi32.Fill(_bufferHdc, new Rectangle(offset, 0, width, _bufferSize.Height));
		}
	}
}
