using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynamicGrid.Data
{
	public struct CellRenderingContext
	{
		public Color? CurrentColor;
		public HorizontalAlignment? CurrentAlignemnt;
		public FontStyle? CurrentFontStyle;
		public Rectangle InvalidatedRect;
	}
}
