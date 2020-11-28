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
		public int Width { get; internal set; }

		protected Column(string header, int width = 50)
		{
			Header = header;
			Width = width;
		}

		public abstract Cell GetCell(TRow row);
	}
}
