using DynamicGrid.Interop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynamicGrid.Buffers
{
	internal sealed class DisplayBuffer : IDisposable
	{
#pragma warning disable IDE0052 // we cannot allow for the Graphics object to be collected as then the associated HDC will get invalidated
		private readonly Graphics _parent;
#pragma warning restore IDE0052
		private readonly IntPtr _parentHdc;

		private BufferedGraphics _buffer;
		private IntPtr _bufferHdc;
		private Size _bufferSize;

		private Color _backgroundColor;

		public DisplayBuffer(Graphics parent)
		{
			_parent = parent;
			_parentHdc = parent.GetHdc();
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

		public void Resize(Size size)
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

		public DisplayBufferContext CreateDrawingContext()
		{
			return new DisplayBufferContext(_bufferHdc);
		}

		public void Flush(Rectangle rectangle, int offsetX)
		{
			if (_buffer != null)
				Gdi32.Copy(_bufferHdc, new Point(rectangle.X + offsetX, rectangle.Y), _parentHdc, rectangle.Location, rectangle.Size);
		}
	}
}
