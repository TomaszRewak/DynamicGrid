using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid.Buffers
{
	internal sealed class DisplayBuffer<TRow>
	{
		private readonly IntPtr _grapthics;

		private BufferedGraphics _buffer;

		public DisplayBuffer(IntPtr grapthics)
		{
			_grapthics = grapthics;
		}

		public void Draw(
			List<TRow> rows,
			List<IColumn<TRow>> columns,
			Rectangle rect)
		{
			//_buffer = BufferedGraphicsManager.Current.Allocate(_grapthics);


		}

		private void AllocateBuffer()
		{

		}
	}
}
