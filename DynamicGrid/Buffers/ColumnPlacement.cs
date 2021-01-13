using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid.Buffers
{
	internal readonly struct ColumnPlacement
	{
		public readonly int Offset;
		public readonly int Width;

		public ColumnPlacement(int offset, int width) : this()
		{
			Offset = offset;
			Width = width;
		}
	}
}
