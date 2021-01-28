using System;
using System.Windows.Forms;

namespace DynamicGrid.ExampleApp
{
	static class Program
	{
		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.SetHighDpiMode(HighDpiMode.SystemAware);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			new TickerForm().Show();
			new FollowerForm().Show();
			new SelectionForm().Show();

			Application.Run(new ColorfulForm());
		}
	}
}
