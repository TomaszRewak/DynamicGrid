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
		private readonly Color _backgroundColor;

		private Color? _currentColor;
		private HorizontalAlignment? _currentAlignemnt;

		public Rectangle InvalidatedRect { get; private set; } = Rectangle.Empty;

		public DisplayBufferContext(IntPtr buffer, Color backgroundColor)
		{
			_buffer = buffer;
			_backgroundColor = backgroundColor;
		}

		public void Draw(int x, int y, int width, int height, in Cell cell)
		{
			var rectangle = new Rectangle(x, y, width - 1, height - 1);
			var position = new Point(width / 2, 0);
			var cellColor = cell.Color.A == byte.MaxValue
				? cell.Color
				: _backgroundColor;

			if (_currentColor != cellColor)
			{
				_currentColor = cellColor;
				Gdi32.SetBackgroundColor(_buffer, cellColor);
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
