using System.Drawing;
using System.Windows.Forms;

namespace DynamicGrid
{
	[System.ComponentModel.DesignerCategory("")]
	public partial class Grid<TRow> : UserControl
	{
		public Grid()
		{
			BackColor = Color.Red;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			e.Graphics.GetHdc();
		}
	}
}
