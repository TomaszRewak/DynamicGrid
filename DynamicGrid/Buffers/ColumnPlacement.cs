using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid.Buffers
{
	internal readonly struct ColumnPlacement
	{
		public readonly int Width;
		public readonly int CroppedIndex;
		public readonly int CroppedOffset;
		public readonly int RealOffset;

		public ColumnPlacement(int width, int croppedIndex, int croppedOffset, int realOffset)
		{
			Width = width;
			CroppedIndex = croppedIndex;
			CroppedOffset = croppedOffset;
			RealOffset = realOffset;
		}
	}
}
