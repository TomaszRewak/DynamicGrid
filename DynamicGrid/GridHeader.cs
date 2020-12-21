using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DynamicGrid
{
	[System.ComponentModel.DesignerCategory("")]
	public class GridHeader<TRow> : Control
	{
		private readonly Control _container;

		private Column<TRow>[] _columns = Array.Empty<Column<TRow>>();
		public IReadOnlyCollection<Column<TRow>> Columns
		{
			get => _columns.ToArray();
			set
			{
				foreach (var column in _columns)
					column.WidthChanged -= OnColumnWidthChanged;

				_columns = value.ToArray();

				foreach (var column in _columns)
					column.WidthChanged += OnColumnWidthChanged;

				Rebuild();
				UpdateColumnsWidth();
			}
		}

		private int _columnWidths;
		public int ColumnsWidths
		{
			get => _columnWidths;
			private set
			{
				if (_columnWidths == value) return;

				_columnWidths = value;

				ColumnsWidthChanged?.Invoke(this, EventArgs.Empty);
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

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Columns = Array.Empty<Column<TRow>>();
			}

			base.Dispose(disposing);
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

			if (topSplitter != null)
			{
				topSplitter.Width = int.MaxValue;
				topSplitter.Dock = DockStyle.Top | DockStyle.Bottom;
				topSplitter.Left = 10;
			}

			_container.Controls.Add(topSplitter);

			ResumeLayout();
		}

		private void UpdateColumnsWidth()
		{
			ColumnsWidths = Columns.Sum(c => c.Width);
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnResize(e);

			_container.Height = Height;
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			_container.Height = Height;
		}

		private void OnColumnWidthChanged(object sender, EventArgs e)
		{
			UpdateColumnsWidth();
		}

		public event EventHandler ColumnsWidthChanged;
	}
}
