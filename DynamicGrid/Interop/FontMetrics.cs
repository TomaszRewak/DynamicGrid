using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid.Interop
{
	internal readonly struct FontMetrics
	{
		public int FontHeight { get; }

		public int FontOffset { get; }

		public FontMetrics(int fontHeight, int fontOffset) : this()
		{
			FontHeight = fontHeight;
			FontOffset = fontOffset;
		}
	}
}
