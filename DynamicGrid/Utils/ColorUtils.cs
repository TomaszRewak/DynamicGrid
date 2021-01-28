using System.Drawing;

namespace DynamicGrid.Utils
{
	internal static class ColorUtils
	{
		public static Color Mix(Color lhs, Color rhs, double ratioOfRhs)
		{
			ratioOfRhs = MathUtils.Clip(0, ratioOfRhs, 1);

			double ratioOfLhs = (1 - ratioOfRhs);

			return Color.FromArgb(
				(int)(lhs.R * ratioOfLhs + rhs.R * ratioOfRhs),
				(int)(lhs.G * ratioOfLhs + rhs.G * ratioOfRhs),
				(int)(lhs.B * ratioOfLhs + rhs.B * ratioOfRhs));
		}
	}
}
