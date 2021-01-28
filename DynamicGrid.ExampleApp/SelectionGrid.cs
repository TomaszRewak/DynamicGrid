using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DynamicGrid.ExampleApp
{
	internal sealed class SelectionGrid : Grid
	{
		private readonly Dictionary<(int Row, int Column), int> _values = new();
		private readonly HashSet<(int Row, int Column)> _selection = new();
		private readonly TextBox _textBox;

		bool _mouseDown;

		public SelectionGrid()
		{
			_textBox = new TextBox();

			_textBox.TextAlign = HorizontalAlignment.Center;
			_textBox.Visible = false;
			_textBox.BackColor = Color.LightGreen;
			_textBox.TextChanged += OnTextChanged;
			_textBox.PreviewKeyDown += OnKeyDown;

			BackColor = Color.LightBlue;
			Controls.Add(_textBox);
			Columns = Enumerable.Repeat(25, 32);
		}

		protected override Cell GetCell(int rowIndex, int columnIndex)
		{
			var cell = new Cell(backgroundColor: Color.White);
			var isSelected = _selection.Contains((rowIndex, columnIndex));

			if (isSelected && _textBox.Text != string.Empty)
			{
				cell = new Cell(_textBox.Text, Color.LightGreen);
			}
			else
			{
				if (_values.TryGetValue((rowIndex, columnIndex), out var value))
					cell = new Cell(value.ToString(), Color.White).Highlight(Color.Red, value / 10f);

				if (isSelected)
					cell = cell.Highlight(Color.Blue, 0.2);
			}

			return cell;
		}

		private void OnKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Escape:
					ClearSelection();
					break;
				case Keys.Delete:
					DeleteSelected();
					break;
				case Keys.Enter:
					Apply((old, value) => value);
					break;
				case Keys.Add:
					Apply((old, value) => old + value);
					break;
				case Keys.Subtract:
					Apply((old, value) => old - value);
					break;
				case Keys.Multiply:
					Apply((old, value) => old * value);
					break;
				case Keys.Divide:
					Apply((old, value) => old / (value == 0 ? 1 : value));
					break;
			}
		}

		protected override void OnMouseDownOverCell(MouseCellEventArgs e)
		{
			base.OnMouseDownOverCell(e);

			if (!_selection.Any())
			{
				_textBox.Size = e.ControlRect.Size;

				var location = e.ControlRect.Location;
				location.Offset(0, -(_textBox.Height - e.ControlRect.Height)/2);

				_textBox.Location = location;
				_textBox.Text = string.Empty;
				_textBox.Visible = true;
				_textBox.Focus();
			}

			Select(e.Row, e.Column);

			_mouseDown = true;
		}

		protected override void OnMouseMovedOverGrid(MouseCellEventArgs e)
		{
			base.OnMouseMovedOverGrid(e);

			if (!_mouseDown) return;

			Select(e.Row, e.Column);
		}

		protected override void OnMouseUpOverCell(MouseCellEventArgs e)
		{
			base.OnMouseUpOverCell(e);

			_mouseDown = false;
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			var offset = -Math.Sign(e.Delta) * RowHeight;

			VerticalOffset += offset;
			_textBox.Location = new Point(_textBox.Location.X, _textBox.Location.Y - offset);
		}

		private void Select(int row, int column)
		{
			var cell = (row, column);

			if (_selection.Contains(cell)) return;

			_selection.Add(cell);

			InvalidateCellData(row, column);
			UpdateData();
		}

		private void OnTextChanged(object sender, EventArgs e)
		{
			InvalidateSelection();
			UpdateData();
		}

		private void ClearSelection()
		{
			InvalidateSelection();

			_selection.Clear();
			_textBox.Visible = false;

			UpdateData();
		}

		private void DeleteSelected()
		{
			foreach (var key in _selection)
				_values.Remove(key);

			ClearSelection();
		}

		private void Apply(Func<int, int, int> transform)
		{
			if (!int.TryParse(_textBox.Text, out var value)) return;

			foreach (var key in _selection)
				_values[key] = transform(_values.GetValueOrDefault(key), value);

			ClearSelection();
		}

		private void InvalidateSelection()
		{
			foreach (var key in _selection)
				InvalidateCellData(key.Row, key.Column);
		}
	}
}
