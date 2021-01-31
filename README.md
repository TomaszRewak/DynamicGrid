# DynamicGrid

DynamicGrid is a high performance WinForms grid rendering engine. It's key features are:
- Rows and columns out of the drawing area and not being render nor their data is being fetched.
- The grid works in a virtual mode - only requesting data for visible cells allowing users to store rows in a collection of choice (also making it possible to implement infinite grids if required).
- The control uses double buffering so that the `OnPaint` methods only calls the `BitBlt` function to flush the prerendered state on the screen (which allows for smooth scrolling).
- When (due to scrolling) new rows or columns come into the view - only those new rows/columns are invalidated and rendered (thanks to a circular display buffer used by the control).
- The grid maintains information about the current state of displayed cells, so if their state does not change after an invalidation, no rendering operations are invoked.
- The user can choose to invalidate only a selected region of the grid.
- The data invalidation follows the invalidate/update/refresh pattern. Different invalidation calls can be aggregated together without causing multiple redraw cycles of overlapping regions.
- Cell rendering is performed using the raw `ExtTextOut` `gdi32` calls.
- The grid strives for memory locality, only allocating buffers of a size required to store the visible date.
- The grid provides cell-based mouse interaction events.

<p align="center">
  <img src="https://github.com//TomaszRewak/DynamicGrid/blob/master/About/example.gif?raw=true" width=600/>
</p>

Limitations of the DynamicGrid:
- The row height must be the same for all rows.
- The cell content and layout options are limited to the following properties: `Text`, `TextAlignment`, `FontStyle`, `BackgroundColor`, `ForegroundColor`.
- The entire grid has to use a single font family.
- The DynamicGrid is only a grid rendering engine and does not provide any column management functionalities. It also doesn't allow for displaying column headers. This can be overcome by using a `DataGridView` placed above this control (used only to display column headers).
- The Dynamic grid, out of the box, does not provide any text input nor cell selection functionalities. Those have to be implemented in derived classes based on individual requirements (which, on the other hand, allows for a great flexibility).
