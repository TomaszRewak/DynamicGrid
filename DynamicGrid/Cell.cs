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
		//HorizontalAlignment Alignment { get; }

		public Color Color { get; }

		public Cell(string text, Color color)
		{
			Text = text;
			Color = color;
		}

		public static bool operator ==(in Cell lhs, in Cell rhs) => string.CompareOrdinal(lhs.Text, rhs.Text) == 0;
		public static bool operator !=(in Cell lhs, in Cell rhs) => !(lhs == rhs);

		public override bool Equals(object obj) => obj is Cell cell && cell == this;

		public override int GetHashCode() => throw new NotImplementedException();
	}
}
