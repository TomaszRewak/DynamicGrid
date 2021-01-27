using System.Drawing;
using System.Windows.Forms;

namespace DynamicGrid
{
	public class MouseCellEventArgs
	{
		public int Row { get; }
		public int Column { get; }
		public MouseButtons MouseButtons { get; }
		public Rectangle GridRect { get; }
		public Rectangle ControlRect { get; }

		public MouseCellEventArgs(int row, int column, MouseButtons mouseButtons, Rectangle gridRect, Rectangle controlRect)
		{
			Row = row;
			Column = column;
			MouseButtons = mouseButtons;
			GridRect = gridRect;
			ControlRect = controlRect;
		}
	}
}
