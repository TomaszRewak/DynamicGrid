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
		public Color Color { get; }

		public Cell(Color color) : this(string.Empty, HorizontalAlignment.Center, color) { }
		public Cell(string text, HorizontalAlignment alignment, Color color)
		{
			Text = text;
			Alignment = alignment;
			Color = color;
		}

		public static bool operator !=(in Cell lhs, in Cell rhs) => !(lhs == rhs);
		public static bool operator ==(in Cell lhs, in Cell rhs) =>
			lhs.Color == rhs.Color &&
			lhs.Alignment == rhs.Alignment &&
			string.CompareOrdinal(lhs.Text, rhs.Text) == 0;

		public override bool Equals(object obj) => obj is Cell cell && cell == this;

		public override int GetHashCode() => throw new NotImplementedException();
	}
}
