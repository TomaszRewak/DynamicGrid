using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynamicGrid
{
	[System.ComponentModel.DesignerCategory("")]
	public partial class Grid<TRow> : UserControl
	{
		public List<TRow> Rows { get; set; }
		public List<IColumn<TRow>> Columns { get; set; }

		public Grid()
		{
			BackColor = Color.Red;

			Task.Delay(1000).ContinueWith(t =>
			{
				BeginInvoke(() => {
					Trace.WriteLine("a");
					Invalidate(new Rectangle(50, 50, 50, 50));
					Invalidate(new Rectangle(100, 100, 50, 50));
					Trace.WriteLine("b");
				});
			});
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
