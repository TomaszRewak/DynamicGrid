using DynamicGrid.Buffers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynamicGrid
{
	[System.ComponentModel.DesignerCategory("")]
	public partial class Grid : UserControl
	{
		private readonly CellBuffer _cellBuffer;
		private readonly DisplayBuffer _displayBuffer;

		public Grid()
		{
			BackColor = Color.Red;

			_gridBuffer = new DisplayBuffer<TRow>();

			_gridBuffer = new GridBuffer<TRow>(Handle);
		}

		public void Flush()
		{
			var rect = _gridBuffer.Update(
				Size,
				CollectionsMarshal.AsSpan(Columns),
				CollectionsMarshal.AsSpan(Rows));

			Invalidate(rect);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			Trace.WriteLine($"{e.ClipRectangle.Left} {e.ClipRectangle.Top} {e.ClipRectangle.Right} {e.ClipRectangle.Bottom}");

			e.Graphics.GetHdc();
		}

		private void BeginInvoke(Action p)
		{
			base.BeginInvoke(p);
		}
	}
}
