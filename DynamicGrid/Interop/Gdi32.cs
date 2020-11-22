using System;
using System.Runtime.InteropServices;

namespace DynamicGrid.Interop
{
	internal static class Gdi32
	{
		[DllImport("gdi32.dll", EntryPoint = "ExtTextOutW")]
		static extern bool ExtTextOut(IntPtr hdc, int X, int Y, uint fuOptions, [In] ref RECT lprc, [MarshalAs(UnmanagedType.LPWStr)] string lpString, uint cbCount, [In] IntPtr lpDx);

		[DllImport("gdi32.dll")]
		public static extern uint SetBkColor(IntPtr hdc, int crColor);

		[DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool BitBlt([In] IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, [In] IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

		public enum TernaryRasterOperations : uint
		{
			SRCCOPY = 0x00CC0020,
			SRCPAINT = 0x00EE0086,
			SRCAND = 0x008800C6,
			SRCINVERT = 0x00660046,
			SRCERASE = 0x00440328,
			NOTSRCCOPY = 0x00330008,
			NOTSRCERASE = 0x001100A6,
			MERGECOPY = 0x00C000CA,
			MERGEPAINT = 0x00BB0226,
			PATCOPY = 0x00F00021,
			PATPAINT = 0x00FB0A09,
			PATINVERT = 0x005A0049,
			DSTINVERT = 0x00550009,
			BLACKNESS = 0x00000042,
			WHITENESS = 0x00FF0062,
			CAPTUREBLT = 0x40000000 //only if WinVer >= 5.0.0 (see wingdi.h)
		}

		[Flags]
		public enum ETOOptions : uint
		{
			ETO_CLIPPED = 0x4,
			ETO_GLYPH_INDEX = 0x10,
			ETO_IGNORELANGUAGE = 0x1000,
			ETO_NUMERICSLATIN = 0x800,
			ETO_NUMERICSLOCAL = 0x400,
			ETO_OPAQUE = 0x2,
			ETO_PDY = 0x2000,
			ETO_RTLREADING = 0x800,
		}

		[StructLayout(LayoutKind.Sequential)]
		public readonly struct RECT
		{
			public readonly int Left;
			public readonly int Top;
			public readonly int Right;
			public readonly int Bottom;

			public RECT(int left, int top, int right, int bottom)
			{
				Left = left;
				Top = top;
				Right = right;
				Bottom = bottom;
			}
		}
	}
}
