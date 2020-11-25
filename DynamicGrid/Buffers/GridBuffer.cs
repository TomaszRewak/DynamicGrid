using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid.Buffers
{
	internal sealed class GridBuffer<TRow>
	{
		private readonly CellBuffer _cellBuffer;
		private readonly DisplayBuffer _displayBuffer;

		public GridBuffer(IntPtr graphics)
		{
			_cellBuffer = new CellBuffer();
			_displayBuffer = new DisplayBuffer(graphics);
		}


	}
}
