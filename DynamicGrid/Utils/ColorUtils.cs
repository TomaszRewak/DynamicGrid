using System.Diagnostics;
using System.Drawing;

namespace DynamicGrid.Utils
{
	internal static class ColorUtils
	{
		public static Color Mix(Color lhs, Color rhs, double ratioOfLhs)
		{
			Debug.Assert(ratioOfLhs >= 0);
			Debug.Assert(ratioOfLhs <= 1);

			double ratioOfRhs = (1 - ratioOfLhs);

			return Color.FromArgb(
				(int)(lhs.R * ratioOfLhs + rhs.R * ratioOfRhs),
				(int)(lhs.G * ratioOfLhs + rhs.G * ratioOfRhs),
				(int)(lhs.B * ratioOfLhs + rhs.B * ratioOfRhs));
		}
	}
}
