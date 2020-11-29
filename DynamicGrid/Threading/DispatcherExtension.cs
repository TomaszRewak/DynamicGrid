using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynamicGrid.Threading
{
	internal static class DispatcherExtension
	{
		public static void DispatchOnce(this Control control, Ref<bool> guard, Action action)
		{
			if (guard.Value) return;

			guard.Value = true;

			control.BeginInvoke(() =>
			{
				guard.Value = false;
				action();
			});
		}

		public static void BeginInvoke(this Control control, Action action)
		{
			control.BeginInvoke(action);
		}
	}
}
