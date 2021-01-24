using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynamicGrid
{
	public class MouseCellEventArgs
	{
		public int Row { get; }
		public int Coulmn { get; }
		public MouseButtons MouseButtons { get; }

		public MouseCellEventArgs(int row, int column, MouseButtons mouseButtons)
		{
			Row = row;
			Coulmn = column;
			MouseButtons = mouseButtons;
		}
	}
}
