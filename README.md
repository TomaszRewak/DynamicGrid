# DynamicGrid

DynamicGrid is a high-performance WinForms grid rendering engine. Its key features are:
- Rows and columns out of the drawing area and not being render nor their data being fetched.
- The grid works in a virtual mode - only requesting data for visible cells allowing users to store rows in a collection of choice (also making it possible to implement infinite grids if required).
- The control uses double buffering so that the `OnPaint` methods only calls the `BitBlt` function to flush the prerendered state on the screen (which allows for smooth scrolling).
- When (due to scrolling) new rows or columns come into the view - only those new rows/columns are invalidated and rendered (thanks to a circular display buffer used by the control).
- The grid maintains information about the current state of displayed cells, so if their state does not change after an invalidation, no rendering operations are invoked.
- The user can choose to invalidate only a selected region of a grid.
- The data invalidation follows the invalidate/update/refresh pattern. Different invalidation calls can be aggregated together without causing multiple redraw cycles of overlapping regions.
- Cell rendering is performed using the raw `ExtTextOut` `gdi32` calls.
- The grid follows the principles of memory locality, only allocating buffers of a size required to store the visible date.
- The grid provides cell-based mouse interaction events.

<p align="center">
  <img src="https://github.com//TomaszRewak/DynamicGrid/blob/master/About/example.gif?raw=true" width=600/>
</p>

Limitations of the DynamicGrid:
- The row height must be the same for all rows.
- The cell content and layout options are limited to the following properties: `Text`, `TextAlignment`, `FontStyle`, `BackgroundColor`, `ForegroundColor`.
- The DynamicGrid is only a grid rendering engine and does not provide any column management functionalities. It also doesn't allow for displaying column headers. This can be overcome by using a `DataGridView` control placed just above our grid (to be used only for displaying the headers).
- The Dynamic grid, out of the box, does not provide any text input nor cell selection functionalities. If required, those must be implemented in derived classes based on individual requirements (which, on the other hand, allows for a greater flexibility).

## Setup and basic usage

First step is to install the package in the project:

```
dotnet add package DynamicGrid
```

After the package is installed, create your custom grid control by inheriting the `abstract` `DynamicGrid.Grid` class.

```csharp
class MyGrid : DynamicGrid.Grid
{
  ...
}
```

The next step will be to set the column sizes (and their number by a proxy) and to override the `GetCell` method of the base `DynamicGrid.Grid` class. The column sizes and their number can be freely changed throughout the lifespan of the grid - but for the purpose of this example we will simply assign them in a constructor.

```csharp
class MyGrid : Grid
{
  public MyGrid()
  {
    Columns = new[] { 150, 150, 150, 150 };
  }

  protected override Cell GetCell(int rowIndex, int columnIndex)
  {
    return new Cell($"{rowIndex} {columnIndex}");
  }
}
```

The `Cell` `struct` conveys the full information about the cell content and layout.

## Data invalidation

If your data is dynamic and changes over time, use the `Invalidate[_|Row|Column|Cell]Data`, `UpdateData` and `RefreshData` methods to control when the grid should be updated.

```csharp
class MyGrid : Grid
{
  ...

  public void UpdateRow(int rowIndex, Row row)
  {
    _rows[rowIndex] = row;

    InvalidateRowData(rowIndex);
    UpdateData();
    Update(); // optional
  }
}
```

The `Invalidate[_|Row|Column|Cell]Data` methods mark a sector of a grid as dirty, the `UpdateData` method redraws the dirty sectors and marks them as clean again and the `RefreshData` method simply combines the two. If you want to immediately flush the pre-rendered state on the screen, call the default `Update` method of the control (or alternatively wait for the next redraw cycle).

## Grid interaction

To handle scrolling within the grid one has to use the `HorizontalOffset` and the `VerticalOffset` properties. By using them (and not placing the grid in a bigger scrollable container) the grid can easily avoid pre-rendering rows and columns that are out of its bounds. Best way to allow users to interact with those properties is to place the `HScrollBar` and the `VScrollBar` next to the grid or to override the `OnMouseWheel` method of the control.

Cell selection and other grid interactions have to be implemented manually, but the grid provides a set of events and virtual method that help with this task:

```csharp
public event EventHandler<EventArgs> ColumnsChanged;
public event EventHandler<EventArgs> HorizontalOffsetChanged;
public event EventHandler<EventArgs> VerticalOffsetChanged;
public event EventHandler<MouseCellEventArgs> CellClicked;
public event EventHandler<MouseCellEventArgs> CellDoubleClicked;
public event EventHandler<MouseCellEventArgs> MouseDownOverCell;
public event EventHandler<MouseCellEventArgs> MouseUpOverCell;
public event EventHandler<MouseCellEventArgs> MouseMovedOverGrid;
public event EventHandler<EventArgs> MouseEnteredGrid;
public event EventHandler<EventArgs> MouseLeftGrid;
```

The following example demonstrates how easy it is to display an interactive text box over a cell after a user clicks on it:

```csharp
class MyGrid : Grid
{
  private TextBox _textBox;
  ...

  protected override void OnMouseDownOverCell(MouseCellEventArgs e)
  {
    base.OnMouseDownOverCell(e);

    _selection = (row, column);

    _textBox.Size = e.ControlRect.Size;
    _textBox.Location = e.ControlRect.Location;
    _textBox.Text = _data[_selection];
    _textBox.Visible = true;
    _textBox.Focus();
  }

  private void textBox_OnKeyDown(object sender, PreviewKeyDownEventArgs e)
  {
    switch (e.KeyCode)
    {
      case Keys.Escape:
        _textBox.Visible = false;
        break;
      case Keys.Enter:
        _data[_selection] = _textBox.Text;
        _textBox.Visible = false;
        InvalidateCellData(_selection.Row, _selection.Column);
        UpdateData();
        break;
    }
  }
}
```

## Summary

As you might have noticed, the DynamicGrid requires the developer to do some manual labor, and so it probably should not be your first choice in most scenarios. But if you need to create a custom high-performance grid, then I would definitely suggest trying it out.

## Contributions

I am open to PRs as well as issues and other questions.
