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
		public Color Color { get; }

		public Cell(Color color) : this(string.Empty, color: color) { }
		public Cell(
			string text,
			Color color,
			HorizontalAlignment alignment = HorizontalAlignment.Center,
			FontStyle fontStyle = FontStyle.Regular)
		{
			Text = text;
			Color = color;
			Alignment = alignment;
			FontStyle = fontStyle;
		}

		public static Cell Empty => new Cell(Color.Transparent);

		public Cell Highlight(Color color, double ratio = 0.5) => new Cell(Text, ColorUtils.Mix(Color, color, ratio), Alignment, FontStyle);

		public static bool operator !=(in Cell lhs, in Cell rhs) => !(lhs == rhs);
		public static bool operator ==(in Cell lhs, in Cell rhs) =>
			lhs.Color == rhs.Color &&
			lhs.Alignment == rhs.Alignment &&
			string.CompareOrdinal(lhs.Text, rhs.Text) == 0;

		public override bool Equals(object obj) => obj is Cell cell && cell == this;

		public override int GetHashCode() => throw new NotImplementedException();
	}
}
