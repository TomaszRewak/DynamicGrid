using DynamicGrid.Interop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid.Managers
{
	internal sealed class FontManager : IDisposable
	{
		private readonly IntPtr _hdc;

		private Font _regularFont;
		private Font _boldFont;

		public IntPtr RegularFont { get; private set; }
		public IntPtr BoldFont { get; private set; }

		public int FontHeight { get; set; }
		public int FontTopOffset { get; set; }
		public int FontBottomOffset { get; set; }

		public FontManager(IntPtr hdc)
		{
			_hdc = hdc;
		}

		~FontManager()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		private void Dispose(bool disposing)
		{
			if (RegularFont != IntPtr.Zero)
			{
				Gdi32.Delete(RegularFont);
				RegularFont = IntPtr.Zero;
			}

			if (BoldFont != IntPtr.Zero)
			{
				Gdi32.Delete(BoldFont);
				BoldFont = IntPtr.Zero;
			}

			if (disposing)
			{
				_regularFont?.Dispose();
				_boldFont?.Dispose();

				_regularFont = null;
				_boldFont = null;
			}
		}

		public void Load(Font font)
		{
			Dispose(true);

			_regularFont = new Font(font, FontStyle.Regular);
			_boldFont = new Font(font, FontStyle.Bold);

			RegularFont = _regularFont.ToHfont();
			BoldFont = _boldFont.ToHfont();

			FontHeight = (int)Math.Ceiling(font.GetHeight());
		}
	}
}
