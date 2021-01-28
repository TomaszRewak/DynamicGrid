using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DynamicGrid.ExampleApp
{
	[DesignerCategory("")]
	internal sealed class FollowerForm : Form
	{
		private readonly FollowerGrid _grid;

		public FollowerForm()
		{
			_grid = new FollowerGrid();

			InitializeComponent();
		}

		private void InitializeComponent()
		{
			SuspendLayout();

			_grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_grid.Location = new Point(0, 0);
			_grid.Name = "grid";
			_grid.Size = new Size(200, 500);
			_grid.TabIndex = 1;

			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(200, 500);
			Controls.Add(_grid);
			Name = "FollowerForm";
			Text = "FollowerForm";

			ResumeLayout(false);
		}
	}
}
