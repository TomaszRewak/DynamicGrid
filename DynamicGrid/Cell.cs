using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid
{
	public readonly struct Cell
	{
		public string Text { get; }

		public static bool operator ==(in Cell lhs, in Cell rhs) => string.CompareOrdinal(lhs.Text, rhs.Text) == 0;
		public static bool operator !=(in Cell lhs, in Cell rhs) => !(lhs == rhs);
	}
}
