using DynamicGrid.Interop;
using System;
using System.ComponentModel;
using System.Drawing;

namespace DynamicGrid.Managers
{
	internal sealed class FontManager : IDisposable
	{
		private sealed class FontWrapper : IDisposable
		{
			private readonly Font _font;
			private bool _disposed;

			public IntPtr Hdc { get; private set; }

			public FontWrapper(Font font, FontStyle fontStyle)
			{
				_font = new Font(font, fontStyle);
				Hdc = _font.ToHfont();
			}

			~FontWrapper()
			{
				Dispose(false);
			}

			public void Dispose()
			{
				Dispose(true);
			}

			private void Dispose(bool disposing)
			{
				if (_disposed) return;
				_disposed = true;

				Gdi32.Delete(Hdc);

				if (disposing)
					_font.Dispose();

				GC.SuppressFinalize(this);
			}
		}

		private FontWrapper _regularFont;
		private FontWrapper _boldFont;
		private FontWrapper _italicFont;
		private FontWrapper _underlineFont;
		private FontWrapper _strikeoutFont;

		public int FontHeight { get; private set; }

		public void Dispose()
		{
			_regularFont?.Dispose();
			_boldFont?.Dispose();
			_italicFont?.Dispose();
			_underlineFont?.Dispose();
			_strikeoutFont?.Dispose();

			_regularFont = null;
			_boldFont = null;
			_italicFont = null;
			_underlineFont = null;
			_strikeoutFont = null;
		}

		public void Load(Font font)
		{
			Dispose();

			FontHeight = (int)Math.Ceiling(font.GetHeight());

			_regularFont = new FontWrapper(font, FontStyle.Regular);
			_boldFont = new FontWrapper(font, FontStyle.Bold);
			_italicFont = new FontWrapper(font, FontStyle.Italic);
			_underlineFont = new FontWrapper(font, FontStyle.Underline);
			_strikeoutFont = new FontWrapper(font, FontStyle.Strikeout);
		}

		public IntPtr GetHdc(FontStyle style)
		{
			return style switch
			{
				FontStyle.Regular => _regularFont.Hdc,
				FontStyle.Bold => _boldFont.Hdc,
				FontStyle.Italic => _italicFont.Hdc,
				FontStyle.Underline => _underlineFont.Hdc,
				FontStyle.Strikeout => _strikeoutFont.Hdc,
				_ => throw new InvalidEnumArgumentException(nameof(style), (int)style, typeof(FontStyle))
			};
		}
	}
}
