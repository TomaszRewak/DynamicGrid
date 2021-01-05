using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid
{
	public abstract class Column
	{
		public string Header { get; }

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

		protected Column(string header)
		{
			Header = header;
		}

		public event EventHandler WidthChanged;
	}
}
