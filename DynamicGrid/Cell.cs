using DynamicGrid.Utils;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DynamicGrid
{
	/// <summary>
	/// Defines the content and the layout of a grid cell.
	/// </summary>
	public readonly struct Cell
	{
		public string Text { get; }
		public HorizontalAlignment TextAlignment { get; }
		public FontStyle FontStyle { get; }
		public Color? BackgroundColor { get; }
		public Color? ForegroundColor { get; }

		public Cell(
			string text = default,
			Color? backgroundColor = null,
			Color? foregroundColor = null,
			HorizontalAlignment textAlignment = HorizontalAlignment.Center,
			FontStyle fontStyle = FontStyle.Regular)
		{
			Text = text;
			BackgroundColor = backgroundColor;
			ForegroundColor = foregroundColor;
			TextAlignment = textAlignment;
			FontStyle = fontStyle;
		}

		/// <summary>
		/// An empty cell that has the same background color as the background color of the grid.
		/// </summary>
		public static Cell Empty => new();
		/// <summary>
		/// Alters the background color of a cell with a specified accent color.
		/// </summary>
		/// <param name="color">Accent color.</param>
		/// <param name="ratio">The ratio in which the accent color should be used.</param>
		/// <returns></returns>
		public Cell Highlight(Color color, double ratio = 0.5) => new(Text, ColorUtils.Mix(BackgroundColor ?? Color.White, color, ratio), ForegroundColor, TextAlignment, FontStyle);

		public static bool operator !=(in Cell lhs, in Cell rhs) => !(lhs == rhs);
		public static bool operator ==(in Cell lhs, in Cell rhs) =>
			lhs.BackgroundColor == rhs.BackgroundColor &&
			lhs.ForegroundColor == rhs.ForegroundColor &&
			lhs.TextAlignment == rhs.TextAlignment &&
			lhs.FontStyle == rhs.FontStyle &&
			string.CompareOrdinal(lhs.Text, rhs.Text) == 0;

		public override bool Equals(object obj) => obj is Cell cell && cell == this;
		public override int GetHashCode() => throw new NotImplementedException();
	}
}
