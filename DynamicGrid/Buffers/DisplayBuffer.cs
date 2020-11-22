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

		public void Draw(int x, int y, in Cell cell)
		{
			Gdi32.SetBkColor(_bufferHdc, cell.Color.ToArgb());
			Gdi32.TextOut(_bufferHdc, )
		}

		private void AllocateBuffer()
		{

		}
	}
}
