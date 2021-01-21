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
			_gridHeader = new GridHeader<MyRow>();
			_grid = new MyGrid();
			_horizontalScrollBar = new HScrollBar();

			SuspendLayout();

			_gridHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_gridHeader.Location = new Point(0, 0);
			_gridHeader.Name = "gridHeader";
			_gridHeader.Size = new Size(800, 40);
			_gridHeader.TabIndex = 0;
			_gridHeader.TotalColumnWidthChanged += OnTotalColumnWidthChanged;
			_gridHeader.ColumnsArrangementChanged += OnColumnsChanged;
			_gridHeader.ColumnsResized += OnColumnsChanged;

			_grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_grid.Location = new Point(0, 40);
			_grid.Name = "grid";
			_grid.Size = new Size(800, 430);
			_grid.TabIndex = 1;

			_horizontalScrollBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			_horizontalScrollBar.Location = new Point(0, 470);
			_horizontalScrollBar.Name = "horizontalScrollBar";
			_horizontalScrollBar.Size = new Size(800, 30);
			_horizontalScrollBar.TabIndex = 2;
			_horizontalScrollBar.ValueChanged += OnHorizontalScrollBarValueChanged;

			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 500);
			Controls.Add(_gridHeader);
			Controls.Add(_grid);
			Controls.Add(_horizontalScrollBar);
			Name = "Form1";
			Text = "Form1";

			ResumeLayout(false);
		}

		private GridHeader<MyRow> _gridHeader;
		private MyGrid _grid;
		private HScrollBar _horizontalScrollBar;
	}
}

