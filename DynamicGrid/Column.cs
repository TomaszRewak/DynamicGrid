using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid
{
	public abstract class Column<TRow>
	{
		private int _width = 50;
		public int Width
		{
			get => _width;
			set
			{
				_width = value;
				WidthChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public abstract Cell GetCell(TRow row);

		public event EventHandler WidthChanged;
	}
}
