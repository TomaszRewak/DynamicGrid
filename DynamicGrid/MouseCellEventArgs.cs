using System.Drawing;
using System.Windows.Forms;

namespace DynamicGrid
{
	public class MouseCellEventArgs
	{
		/// <summary>
		/// Index of a row over which the event took place.
		/// </summary>
		public int Row { get; }
		/// <summary>
		/// Index of a column over which the event took place.
		/// </summary>
		public int Column { get; }
		/// <summary>
		/// The mouse buttons pressed when the event took place.
		/// </summary>
		public MouseButtons MouseButtons { get; }
		/// <summary>
		/// Position and size of a cell over which the event took place in relation to the top left cornet of a cell with coordinates (0, 0).
		/// </summary>
		public Rectangle GridRect { get; }
		/// <summary>
		/// Position and size of a cell over which the event took place in relation to the top left cornet of the grid control.
		/// </summary>
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
