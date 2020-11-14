namespace DynamicGrid
{
	public interface IColumn<TRow>
	{
		Cell Header { get; }

		int Width { get; }

		Cell GetValue(TRow row)
		{
			return new Cell();
		}
	}
}
