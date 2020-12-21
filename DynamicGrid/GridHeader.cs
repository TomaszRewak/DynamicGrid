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
		private readonly object _dragData = new();

		private int? _draggedColumn;

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

				ColumnsChanged?.Invoke(this, EventArgs.Empty);
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

		public bool IsCulumnReorderingEnabled { get; set; }
		public bool IsCulumnDeletionEnabled { get; set; }

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
			SplitContainer topSplitter = null;
			for (int i = _columns.Length - 1; i >= 0; i--)
			{
				var columnIndex = i;
				var column = _columns[columnIndex];
				var label = new Label
				{
					Text = column.Header,
					ForeColor = Color.White,
					BackColor = Color.FromArgb(50, 50, 50),
					Dock = DockStyle.Fill,
					TextAlign = ContentAlignment.MiddleCenter,
					Padding = Padding.Empty,
					AllowDrop = true,
				};
				label.MouseDown += (s, e) =>
				{
					_draggedColumn = columnIndex;

					if (DoDragDrop(_dragData, DragDropEffects.Move) != DragDropEffects.Move)
						RemoveColumn(columnIndex);
				};
				label.MouseUp += (s, e) =>
				{
					if (_draggedColumn == null) return;

					if (e.Y < 0)
						RemoveColumn(_draggedColumn.Value);

					_draggedColumn = null;
				};
				label.DragEnter += (s, e) =>
				{
					if (e.Data.GetData(typeof(object)) != _dragData) return;

					e.Effect = DragDropEffects.Move;
				};
				label.DragDrop += (s, e) =>
				{
					if (e.Data.GetData(typeof(object)) != _dragData) return;
					if (_draggedColumn == null) return;

					MoveColumn(_draggedColumn.Value, columnIndex);
				};

				var splitter = new SplitContainer
				{
					Dock = DockStyle.Fill,
					FixedPanel = FixedPanel.Panel1,
					SplitterDistance = column.Width - 3,
					SplitterWidth = 3
				};
				splitter.Panel1.Controls.Add(label);
				splitter.Panel2.Controls.Add(topSplitter);
				splitter.SplitterMoved += (s, e) =>
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

			SuspendLayout();

			_container.Controls.Clear();
			_container.Controls.Add(topSplitter);

			ResumeLayout(true);
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

		private void MoveColumn(int from, int to)
		{
			var column = _columns[from];
			var columns = new List<Column<TRow>>(_columns);

			columns.RemoveAt(from);
			columns.Insert(to, column);

			Columns = columns;
		}

		private void RemoveColumn(int index)
		{
			var columns = new List<Column<TRow>>(_columns);

			columns.RemoveAt(index);

			Columns = columns;
		}

		public event EventHandler ColumnsWidthChanged;
		public event EventHandler ColumnsChanged;
	}
}
