using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid
{
	public interface IDataSupplier
	{
		Cell Get(int row, int column);
	}
}
