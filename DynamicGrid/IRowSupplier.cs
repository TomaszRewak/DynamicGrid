using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid
{
	public interface IRowSupplier<TRow>
	{
		TRow Get(int row);
	}
}
