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

		private void Rebuild()
		{
			SuspendLayout();

			Controls.Clear();

			SplitContainer topSplitter = null;
			foreach (var column in Columns.Reverse())
			{
				var splitter = new SplitContainer();
				splitter.Panel1.Controls.Add(new Label { Text = column.Header });
				splitter.Panel2.Controls.Add(topSplitter);
				splitter.SplitterMoved += (o, e) => {
					column.Width = splitter.SplitterDistance;
				};

				topSplitter = splitter;
			}

			Controls.Add(topSplitter);

			topSplitter.Dock = DockStyle.Fill;

			ResumeLayout();
		}
	}
}
