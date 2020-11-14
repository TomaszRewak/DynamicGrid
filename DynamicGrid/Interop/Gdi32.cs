using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid.Interop
{
	internal static class Gdi32
	{
		[DllImport("gdi32.dll", CharSet = CharSet.Auto)]
		static extern bool TextOut(IntPtr hdc, int nXStart, int nYStart, string lpString, int cbString);

		[DllImport("gdi32.dll")]
		static extern uint SetBkColor(IntPtr hdc, int crColor);
	}
}
