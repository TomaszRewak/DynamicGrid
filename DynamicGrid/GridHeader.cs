using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynamicGrid
{
	[System.ComponentModel.DesignerCategory("")]
	public class GridHeader<TRow> : UserControl
	{
		private Column<TRow>[] _columns = Array.Empty<Column<TRow>>();
		public IReadOnlyCollection<Column<TRow>> Columns
		{
			get => _columns.ToArray();
			set
			{
				_columns = value.ToArray();
				Rebuild();
			}
		}

		public int HorizontalOffset
		{
			get => -Padding.Left;
			set => Padding = new Padding(-value, 0, 0, 0);
		}

		public GridHeader()
		{
			BackColor = Color.FromArgb(90, 90, 90);
		}

		private void Rebuild()
		{
			SuspendLayout();

			Controls.Clear();

			SplitContainer topSplitter = null;
			foreach (var column in _columns.Reverse())
			{
				var splitter = new SplitContainer
				{
					Dock = DockStyle.Fill,
					SplitterDistance = column.Width - 3,
					SplitterWidth = 3,
					FixedPanel = FixedPanel.Panel1
				};
				splitter.Panel1.Controls.Add(new Label
				{
					Text = column.Header,
					ForeColor = Color.White,
					BackColor = Color.FromArgb(50, 50, 50),
					Dock = DockStyle.Fill,
					TextAlign = ContentAlignment.MiddleCenter,
					Padding = Padding.Empty
				});
				splitter.Panel2.Controls.Add(topSplitter);
				splitter.SplitterMoved += (o, e) =>
				{
					column.Width = splitter.SplitterDistance + 3;
				};

				topSplitter = splitter;
			}

			Controls.Add(topSplitter);

			topSplitter.Width = int.MaxValue;
			topSplitter.Dock = DockStyle.Left;

			ResumeLayout();
		}
	}
}
