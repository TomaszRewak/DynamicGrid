using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid
{
	public abstract class NamedColumn<TRow> : Column<TRow>
	{
		public string Header { get; }

		public NamedColumn(string header)
		{
			Header = header;
		}
	}
}
