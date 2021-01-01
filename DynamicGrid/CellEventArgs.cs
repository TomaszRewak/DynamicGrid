using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid
{
	public class CellEventArgs : EventArgs
	{
		public int Coulmn { get; init; }
		public int Row { get; init; }
	}
}
