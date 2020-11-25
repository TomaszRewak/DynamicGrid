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

		private Size _desiredSize;
		private Size _actualSize;

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
			if (size.Width <= _actualSize.Width && size.Height <= _actualSize.Height)
			{
				_desiredSize = size;
				return;
			}

			var newSize = new Size(
				Math.Max(size.Width, _actualSize.Width),
				Math.Max(size.Height, _actualSize.Height));
			var newBuffer = BufferedGraphicsManager.Current.Allocate(_parentHdc, new Rectangle(new Point(), newSize));
			var newBufferHdc = newBuffer.Graphics.GetHdc();

			if (_buffer != null)
			{
				Gdi32.BitBlt(newBufferHdc, 0, 0, _actualSize.Width, _actualSize.Height, _bufferHdc, 0, 0, Gdi32.TernaryRasterOperations.NONE);
			}

			Dispose();

			_buffer = newBuffer;
			_bufferHdc = newBufferHdc;
		}

		public void Draw(int x, int y, int width, int height, in Cell cell)
		{
			var rect = new Gdi32.RECT(x, y, x + width, y + height);

			Gdi32.SetBkColor(_bufferHdc, cell.Color.ToArgb());
			Gdi32.ExtTextOut(_bufferHdc, x, y, Gdi32.ETOOptions.CLIPPED, ref rect, cell.Text, (uint)cell.Text.Length, IntPtr.Zero);
		}

		public void Flush(Rectangle rectangle)
		{
			Gdi32.BitBlt(_parentHdc, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, _bufferHdc, rectangle.X, rectangle.Y, Gdi32.TernaryRasterOperations.NONE);
		}
	}
}
