using System.Drawing;

namespace DynamicGrid.Utils
{
	public static class RectangleUtils
	{
		public static Rectangle Union(Rectangle lhs, Rectangle rhs)
		{
			if (HasZeroDimension(lhs))
				return Normalize(rhs);

			if (HasZeroDimension(rhs))
				return Normalize(lhs);

			return Rectangle.Union(lhs, rhs);
		}

		private static bool HasZeroDimension(Rectangle rect) => rect.Width == 0 || rect.Height == 0;

		private static Rectangle Normalize(Rectangle rect) => HasZeroDimension(rect)
			? Rectangle.Empty
			: rect;
	}
}
