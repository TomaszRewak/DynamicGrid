using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid
{
	public sealed class Column
	{
		public string Header { get; init; }
		public int Id { get; init; }

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

		public event EventHandler WidthChanged;
	}
}
