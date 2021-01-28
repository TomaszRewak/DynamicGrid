using System;

namespace DynamicGrid.Utils
{
	internal static class MathUtils
	{
		public static int Clip(int min, int value, int max)
		{
			return Math.Max(min, Math.Min(value, max));
		}

		public static double Clip(double min, double value, double max)
		{
			return Math.Max(min, Math.Min(value, max));
		}
	}
}
