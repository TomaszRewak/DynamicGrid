using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DynamicGrid.ExampleApp
{
	[DesignerCategory("")]
	partial class Form1
	{
		private void InitializeComponent()
		{
			_gridHeader = new MyGridHeader();
			_grid = new MyGrid();
			_horizontalScrollBar = new HScrollBar();
			_verticalScrollBar = new VScrollBar();

			SuspendLayout();

			_gridHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_gridHeader.Location = new Point(0, 0);
			_gridHeader.Name = "gridHeader";
			_gridHeader.Size = new Size(770, 40);
			_gridHeader.TabIndex = 0;
			_gridHeader.ColumnsWidthChanged += OnColumnsWidthChanged;
			_gridHeader.ColumnsChanged += OnColumnsChanged;

			_grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_grid.Location = new Point(0, 40);
			_grid.Name = "grid";
			_grid.Size = new Size(770, 430);
			_grid.TabIndex = 1;

			_horizontalScrollBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_horizontalScrollBar.Location = new Point(0, 470);
			_horizontalScrollBar.Name = "horizontalScrollBar";
			_horizontalScrollBar.Size = new Size(770, 30);
			_horizontalScrollBar.TabIndex = 2;
			_horizontalScrollBar.ValueChanged += OnHorizontalScrollBarValueChanged;

			_verticalScrollBar.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
			_verticalScrollBar.Location = new Point(770, 40);
			_verticalScrollBar.Name = "verticalScrollBar";
			_verticalScrollBar.Size = new Size(30, 430);
			_verticalScrollBar.TabIndex = 3;
			_verticalScrollBar.ValueChanged += OnVerticalScrollBarValueChanged;

			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 500);
			Controls.Add(_gridHeader);
			Controls.Add(_grid);
			Controls.Add(_horizontalScrollBar);
			Controls.Add(_verticalScrollBar);
			Name = "Form1";
			Text = "Form1";

			ResumeLayout(false);
		}

		private MyGridHeader _gridHeader;
		private MyGrid _grid;
		private HScrollBar _horizontalScrollBar;
		private VScrollBar _verticalScrollBar;
	}
}

