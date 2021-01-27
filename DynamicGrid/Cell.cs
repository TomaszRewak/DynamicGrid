using DynamicGrid.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynamicGrid
{
	public readonly struct Cell
	{
		public string Text { get; }
		public HorizontalAlignment Alignment { get; }
		public FontStyle FontStyle { get; }
		public Color? BackgroundColor { get; }
		public Color? ForegroundColor { get; }

		public Cell(
			string text = default,
			Color? backgroundColor = null,
			Color? foregroundColor = null,
			HorizontalAlignment alignment = HorizontalAlignment.Center,
			FontStyle fontStyle = FontStyle.Regular)
		{
			Text = text;
			BackgroundColor = backgroundColor;
			ForegroundColor = foregroundColor;
			Alignment = alignment;
			FontStyle = fontStyle;
		}

		public static Cell Empty => new();
		public Cell Highlight(Color color, double ratio = 0.5) => new(Text, ColorUtils.Mix(BackgroundColor ?? Color.White, color, ratio), ForegroundColor, Alignment, FontStyle);

		public static bool operator !=(in Cell lhs, in Cell rhs) => !(lhs == rhs);
		public static bool operator ==(in Cell lhs, in Cell rhs) =>
			lhs.BackgroundColor == rhs.BackgroundColor &&
			lhs.ForegroundColor == rhs.ForegroundColor &&
			lhs.Alignment == rhs.Alignment &&
			lhs.FontStyle == rhs.FontStyle &&
			string.CompareOrdinal(lhs.Text, rhs.Text) == 0;

		public override bool Equals(object obj) => obj is Cell cell && cell == this;

		public override int GetHashCode() => throw new NotImplementedException();
	}
}
