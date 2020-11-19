namespace DynamicGrid
{
	public interface IColumn<TRow>
	{
		Cell Header { get; }

		Cell GetValue(TRow row)
		{
			return new Cell();
		}
	}
}
