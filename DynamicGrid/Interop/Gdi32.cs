using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DynamicGrid.Interop
{
	internal static class Gdi32
	{
		public static void SetBackgroundColor(IntPtr hdc, Color color)
		{
			var value = (((color.B << 8) | color.G) << 8) | color.R;
			SetBkColor(hdc, value);
		}
		public static void SetForegroundColor(IntPtr hdc, Color color)
		{
			var value = (((color.B << 8) | color.G) << 8) | color.R;
			SetTextColor(hdc, value);
		}

		public static void Copy(IntPtr source, Point sourcePosition, IntPtr target, Point targetPosition, Size size)
		{
			if (size.Width == 0 || size.Height == 0) return;

			BitBlt(target, targetPosition.X, targetPosition.Y, size.Width, size.Height, source, sourcePosition.X, sourcePosition.Y, TernaryRasterOperations.SRCCOPY);
		}

		public static void SetTextAlignemnt(IntPtr hdc, HorizontalAlignment alignment)
		{
			var value = alignment switch
			{
				HorizontalAlignment.Left => Alignment.LEFT,
				HorizontalAlignment.Center => Alignment.CENTER,
				HorizontalAlignment.Right => Alignment.RIGHT,
				_ => throw new InvalidEnumArgumentException(nameof(alignment), (int)alignment, typeof(HorizontalAlignment))
			};

			SetTextAlign(hdc, value);
		}

		public static void PrintText(IntPtr hdc, Rectangle rectangle, HorizontalAlignment alignment, string text)
		{
			text ??= string.Empty;

			var rect = new RECT(rectangle.X, rectangle.Y, rectangle.Right, rectangle.Bottom);
			var position = alignment switch
			{
				HorizontalAlignment.Left => new Point(2, 0),
				HorizontalAlignment.Right => new Point(rectangle.Width - 2, 0),
				HorizontalAlignment.Center => new Point(rectangle.Width / 2, 0),
				_ => throw new InvalidEnumArgumentException(nameof(alignment), (int)alignment, typeof(HorizontalAlignment))
			};

			ExtTextOut(hdc, rectangle.X + position.X, rectangle.Y + position.Y, ETOOptions.OPAQUE | ETOOptions.CLIPPED, ref rect, text, (uint)text.Length, IntPtr.Zero);
		}

		public static void Fill(IntPtr hdc, Rectangle rectangle)
		{
			PrintText(hdc, rectangle, HorizontalAlignment.Center, string.Empty);
		}

		public static void Delete(IntPtr hdc)
		{
			DeleteObject(hdc);
		}

		public static void Select(IntPtr hdc, IntPtr obj)
		{
			SelectObject(hdc, obj);
		}

		public static Size Measure(IntPtr hdc, string text, IntPtr font)
		{
			Select(hdc, font);

			GetTextExtentPoint32(hdc, text, text.Length, out var size);

			return new Size(size.cx, size.cy);
		}

		[DllImport("gdi32.dll", EntryPoint = "ExtTextOutW")]
		private static extern bool ExtTextOut(IntPtr hdc, int X, int Y, ETOOptions fuOptions, [In] ref RECT lprc, [MarshalAs(UnmanagedType.LPWStr)] string lpString, uint cbCount, [In] IntPtr lpDx);

		[DllImport("gdi32.dll")]
		private static extern uint SetBkColor(IntPtr hdc, int crColor);

		[DllImport("gdi32.dll")]
		private static extern uint SetTextColor(IntPtr hdc, int crColor);

		[DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool BitBlt([In] IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, [In] IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

		[DllImport("gdi32.dll")]
		private static extern uint SetTextAlign(IntPtr hdc, Alignment fMode);

		[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool DeleteObject([In] IntPtr hObject);

		[DllImport("gdi32.dll", EntryPoint = "SelectObject")]
		public static extern IntPtr SelectObject([In] IntPtr hdc, [In] IntPtr hgdiobj);

		[DllImport("gdi32.dll")]
		static extern bool GetTextExtentPoint32(IntPtr hdc, string lpString, int cbString, out SIZE lpSize);

		private enum TernaryRasterOperations : uint
		{
			NONE = 0x0,
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
			CAPTUREBLT = 0x40000000
		}

		[Flags]
		private enum ETOOptions : uint
		{
			CLIPPED = 0x4,
			GLYPH_INDEX = 0x10,
			IGNORELANGUAGE = 0x1000,
			NUMERICSLATIN = 0x800,
			NUMERICSLOCAL = 0x400,
			OPAQUE = 0x2,
			PDY = 0x2000,
			RTLREADING = 0x800,
		}

		[Flags]
		private enum Alignment : uint
		{
			LEFT = 0,
			RIGHT = 2,
			CENTER = 6,

			TOP = 0,
			BOTTOM = 8,
			BASELINE = 24
		}

		[StructLayout(LayoutKind.Sequential)]
		private readonly struct RECT
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

		[StructLayout(LayoutKind.Sequential)]
		public struct SIZE
		{
			public int cx;
			public int cy;

			public SIZE(int cx, int cy)
			{
				this.cx = cx;
				this.cy = cy;
			}
		}
	}
}
