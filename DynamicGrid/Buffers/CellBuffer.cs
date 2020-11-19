using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid.Buffers
{
	internal sealed class CellBuffer
	{
		private (bool Changed, Cell State)[,] _cells = new (bool Changed, Cell State)[0, 0];

		public void Resize()
	}
}
