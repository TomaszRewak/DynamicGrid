using DynamicGrid.Utils;
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

		private IReadOnlyList<NamedColumn<TRow>> _columns = Array.Empty<NamedColumn<TRow>>();
		public IReadOnlyList<NamedColumn<TRow>> Columns
		{
			get => _columns;
			set
			{
				foreach (var column in _columns)
					column.WidthChanged -= OnColumnWidthChanged;

				_columns = value.ToList().AsReadOnly();

				foreach (var column in _columns)
					column.WidthChanged += OnColumnWidthChanged;

				Rebuild();
				UpdateColumnsWidth();

				ColumnsChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		private int _totalColumnWidth;
		public int TotalColumnWdith
		{
			get => _totalColumnWidth;
			private set
			{
				if (_totalColumnWidth == value) return;

				_totalColumnWidth = value;

				TotalColumnWidthChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public int HorizontalOffset
		{
			get => -_container.Left;
			set => _container.Left = -value;
		}

		public Color SplitterColor
		{
			get => _container.BackColor;
			set => _container.BackColor = value;
		}

		private Color _headerColor = Color.FromArgb(50, 50, 50);
		public Color HeaderColor
		{
			get => _headerColor;
			set
			{
				if (_headerColor == value) return;

				_headerColor = value;
				Rebuild();
			}
		}

		public Color HighlightColor { get; set; } = Color.DarkGreen;

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

			Controls.Add(_container);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Columns = Array.Empty<NamedColumn<TRow>>();
			}

			base.Dispose(disposing);
		}

		private void Rebuild()
		{
			SplitContainer topSplitter = null;
			for (int i = Columns.Count - 1; i >= 0; i--)
			{
				var columnIndex = i;
				var column = Columns[columnIndex];
				var label = new Label
				{
					Text = column.Header,
					ForeColor = Color.White,
					BackColor = HeaderColor,
					Dock = DockStyle.Fill,
					TextAlign = ContentAlignment.MiddleCenter,
					Padding = Padding.Empty,
					AllowDrop = true,
				};
				label.MouseDown += (s, e) =>
				{
					_draggedColumn = columnIndex;

					label.BackColor = ColorUtils.Mix(HeaderColor, HighlightColor, 0.5);
					var result = DoDragDrop(_dragData, DragDropEffects.Move);
					label.BackColor = HeaderColor;

					if (result != DragDropEffects.Move)
						RemoveColumn(columnIndex);
				};
				label.DragEnter += (s, e) =>
				{
					if (e.Data.GetData(typeof(object)) != _dragData) return;

					e.Effect = DragDropEffects.Move;

					if (_draggedColumn != columnIndex)
						label.BackColor = HighlightColor;
				};
				label.DragLeave += (s, e) =>
				{
					if (_draggedColumn != columnIndex)
						label.BackColor = HeaderColor;
				};
				label.DragDrop += (s, e) =>
				{
					if (e.Data.GetData(typeof(object)) != _dragData) return;
					if (_draggedColumn == columnIndex) return;
					if (_draggedColumn == null) return;

					MoveColumn(_draggedColumn.Value, columnIndex);
				};

				var splitter = new SplitContainer
				{
					Dock = DockStyle.Fill,
					Width = int.MaxValue,
					FixedPanel = FixedPanel.Panel1,
					SplitterDistance = column.Width - 3,
					SplitterWidth = 3,
					AllowDrop = true
				};
				splitter.Panel1.Controls.Add(label);
				splitter.Panel2.Controls.Add(topSplitter);
				splitter.SplitterMoved += (s, e) =>
				{
					column.Width = splitter.SplitterDistance + 3;
				};
				splitter.DragEnter += (s, e) =>
				{
					if (e.Data.GetData(typeof(object)) != _dragData) return;

					e.Effect = DragDropEffects.Move;
				};
				splitter.DragDrop += (s, e) =>
				{
					if (e.Data.GetData(typeof(object)) != _dragData) return;
					if (_draggedColumn == null) return;

					MoveColumn(_draggedColumn.Value, columnIndex);
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
			TotalColumnWdith = Columns.Sum(c => c.Width);
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
			var column = Columns[from];
			var columns = new List<NamedColumn<TRow>>(Columns);

			columns.RemoveAt(from);
			columns.Insert(to, column);

			Columns = columns;
		}

		private void RemoveColumn(int index)
		{
			var columns = new List<NamedColumn<TRow>>(Columns);

			columns.RemoveAt(index);

			Columns = columns;
		}

		public event EventHandler ColumnsChanged;
		public event EventHandler TotalColumnWidthChanged;
	}
}
