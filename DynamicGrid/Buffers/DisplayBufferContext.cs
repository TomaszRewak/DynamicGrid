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
	internal sealed class DisplayBufferContext
	{
		private readonly IntPtr _buffer;

		private Color? _currentColor;
		private HorizontalAlignment? _currentAlignemnt;

		public Rectangle InvalidatedRect { get; private set; } = Rectangle.Empty;

		public DisplayBufferContext(IntPtr buffer)
		{
			_buffer = buffer;
		}

		public void Draw(int x, int y, int width, int height, in Cell cell)
		{
			var rectangle = new Rectangle(x, y, width - 1, height - 1);
			var position = new Point(width / 2, 0);

			if (_currentColor != cell.Color)
			{
				_currentColor = cell.Color;
				Gdi32.SetBackgroundColor(_buffer, cell.Color);
			}

			if (_currentAlignemnt != cell.Alignment)
			{
				_currentAlignemnt = cell.Alignment;
				Gdi32.SetTextAlignemnt(_buffer, cell.Alignment);
			}

			Gdi32.PrintText(_buffer, rectangle, position, cell.Text);

			InvalidatedRect = Rectangle.Union(InvalidatedRect, rectangle);
		}
	}
}
