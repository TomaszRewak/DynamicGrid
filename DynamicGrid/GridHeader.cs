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
		private readonly Control _container;

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
			get => -_container.Left;
			set => _container.Left = -value;
		}

		public GridHeader()
		{
			_container = new Panel
			{
				Height = Height,
				Width = int.MaxValue,
				BackColor = Color.FromArgb(90, 90, 90),
				Padding = new Padding(1, 0, 0, 0)
			};

			BackColor = Color.Pink;
			Controls.Add(_container);
		}

		private void Rebuild()
		{
			SuspendLayout();

			_container.Controls.Clear();

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

			topSplitter.Width = int.MaxValue;
			topSplitter.Dock = DockStyle.Top | DockStyle.Bottom;
			topSplitter.Left = 10;

			_container.Controls.Add(topSplitter);

			ResumeLayout();
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnResize(e);

			_container.Height = Height;
		}
	}
}
