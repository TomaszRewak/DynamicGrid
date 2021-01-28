using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DynamicGrid.ExampleApp
{
	[DesignerCategory("")]
	public partial class TickerForm : Form
	{
		private readonly TickerGrid _grid;

		public TickerForm()
		{
			_grid = new TickerGrid();

			InitializeComponent();
		}

		private void InitializeComponent()
		{
			SuspendLayout();

			_grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_grid.Location = new Point(0, 0);
			_grid.Name = "grid";
			_grid.Size = new Size(350, 700);
			_grid.TabIndex = 1;

			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(350, 700);
			Controls.Add(_grid);
			Name = "TickerForm";
			Text = "TickerForm";

			ResumeLayout(false);
		}
	}
}
