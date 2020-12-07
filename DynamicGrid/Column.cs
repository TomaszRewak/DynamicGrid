using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid
{
	public abstract class Column<TRow>
	{
		public string Header { get; }

		private int _width;
		public int Width
		{
			get => _width;
			internal set
			{
				_width = value;
				WidthChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		protected Column(string header, int width = 50)
		{
			Header = header;
			Width = width;
		}

		public abstract Cell GetCell(TRow row);

		public event EventHandler WidthChanged;
	}
}
