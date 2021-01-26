using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DynamicGrid.ExampleApp
{
	[DesignerCategory("")]
	internal sealed class ColorfulForm : Form
	{
		private readonly DataGridView _gridHeader;
		private readonly ColorfulGrid _grid;
		private readonly HScrollBar _horizontalScrollBar;

		public ColorfulForm()
		{
			_gridHeader = new DataGridView();
			_grid = new ColorfulGrid();
			_horizontalScrollBar = new HScrollBar();

			InitializeComponents();

			_gridHeader.Columns.AddRange(Enumerable.Range(0, 200).Select(c => new DataGridViewTextBoxColumn()
			{
				HeaderText = $"Column {c}",
				Width = 150
			}).ToArray());
		}

		private void InitializeComponents()
		{
			SuspendLayout();

			_gridHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			_gridHeader.Location = new Point(0, 0);
			_gridHeader.Name = "gridHeader";
			_gridHeader.Size = new Size(800, 40);
			_gridHeader.TabIndex = 0;
			_gridHeader.RowHeadersVisible = false;
			_gridHeader.ColumnHeadersHeight = 40;
			_gridHeader.BorderStyle = BorderStyle.None;
			_gridHeader.ColumnWidthChanged += OnColumnsChanged;
			_gridHeader.ColumnDisplayIndexChanged += OnColumnsChanged;
			_gridHeader.ColumnAdded += OnColumnsChanged;
			_gridHeader.ColumnRemoved += OnColumnsChanged;

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
			_horizontalScrollBar.Minimum = -50;

			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 500);
			Controls.Add(_gridHeader);
			Controls.Add(_grid);
			Controls.Add(_horizontalScrollBar);
			Name = "ColorfulForm";
			Text = "ColorfulForm";

			ResumeLayout(false);
		}

		private void OnHorizontalScrollBarValueChanged(object sender, EventArgs e)
		{
			_grid.HorizontalOffset = _horizontalScrollBar.Value;
			_gridHeader.HorizontalScrollingOffset = Math.Max(0, _horizontalScrollBar.Value);
		}

		private void OnColumnsChanged(object sender, DataGridViewColumnEventArgs e)
		{
			_grid.Columns = _gridHeader.Columns.OfType<DataGridViewColumn>().Select(c => c.Width);
			_horizontalScrollBar.Maximum = _grid.Columns.Sum() + 500;
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);

			_horizontalScrollBar.LargeChange = ClientSize.Width;
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);

			_grid.VerticalOffset -= Math.Sign(e.Delta) * 4;
		}
	}
}
