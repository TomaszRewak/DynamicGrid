using System.Drawing;
using System.Windows.Forms;

namespace DynamicGrid.Data
{
	public struct CellRenderingContext
	{
		public Color? CurrentBackgroundColor;
		public Color? CurrentForegroundColor;
		public HorizontalAlignment? CurrentAlignemnt;
		public FontStyle? CurrentFontStyle;
		public Rectangle InvalidatedRect;
	}
}
