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
		private readonly IntPtr _parentHdc;

		private BufferedGraphics _buffer;
		private IntPtr _bufferHdc;
		private Size _bufferSize;

		public DisplayBuffer(IntPtr mainHdc)
		{
			_parentHdc = mainHdc;
		}

		public void Dispose()
		{
			if (_buffer == null) return;

			_buffer.Graphics.ReleaseHdc(_bufferHdc);
			_buffer.Dispose();

			_buffer = null;
			_bufferHdc = IntPtr.Zero;
		}

		public void Resize(Size size)
		{
			if (size.Width <= _bufferSize.Width && size.Height <= _bufferSize.Height)
				return;

			var newBufferSize = new Size(
				Math.Max(size.Width, _bufferSize.Width),
				Math.Max(size.Height, _bufferSize.Height));
			var newBuffer = BufferedGraphicsManager.Current.Allocate(_parentHdc, new Rectangle(new Point(), newBufferSize));
			var newBufferHdc = newBuffer.Graphics.GetHdc();

			if (_buffer != null)
				Gdi32.BitBlt(newBufferHdc, 0, 0, _bufferSize.Width, _bufferSize.Height, _bufferHdc, 0, 0, Gdi32.TernaryRasterOperations.NONE);

			Dispose();

			_buffer = newBuffer;
			_bufferHdc = newBufferHdc;
			_bufferSize = newBufferSize;
		}

		public void Draw(int x, int y, int width, int height, in Cell cell)
		{
			var rect = new Gdi32.RECT(x, y, x + width, y + height);

			Gdi32.SetBkColor(_bufferHdc, cell.Color.ToArgb());
			Gdi32.ExtTextOut(_bufferHdc, x, y, Gdi32.ETOOptions.CLIPPED, ref rect, cell.Text, (uint)cell.Text.Length, IntPtr.Zero);
		}

		public void Flush(Rectangle rectangle)
		{
			if (_buffer != null)
				Gdi32.BitBlt(_parentHdc, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, _bufferHdc, rectangle.X, rectangle.Y, Gdi32.TernaryRasterOperations.NONE);
		}
	}
}
