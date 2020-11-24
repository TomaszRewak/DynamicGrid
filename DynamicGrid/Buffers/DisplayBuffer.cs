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
	internal sealed class DisplayBuffer<TRow>
	{
		private readonly IntPtr _mainHdc;

		private BufferedGraphics _buffer;
		private IntPtr _bufferHdc;

		public DisplayBuffer(IntPtr mainHdc)
		{
			_mainHdc = mainHdc;
			_buffer = BufferedGraphicsManager.Current.Allocate(mainHdc, new Rectangle(0, 0, 100, 100));
			_bufferHdc = _buffer.Graphics.GetHdc();
		}

		public void Draw(int x, int y, int width, int height, in Cell cell)
		{
			var rect = new Gdi32.RECT(x, y, x + width, y + height);

			Gdi32.SetBkColor(_bufferHdc, cell.Color.ToArgb());
			Gdi32.ExtTextOut(_bufferHdc, x, y, Gdi32.ETOOptions.CLIPPED, ref rect, cell.Text, (uint)cell.Text.Length, IntPtr.Zero);
		}

		private void AllocateBuffer()
		{

		}
	}
}
