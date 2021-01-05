using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicGrid
{
	public sealed class ColumnCollection<TColumn> : IList<TColumn> where TColumn : Column
	{
		private readonly List<TColumn> _items = new();

		public int TotalWidth { get; set; }

		public TColumn this[int index]
		{
			get => _items[index];
			set
			{
				if (Equals(_items[index], value)) return;

				Unregister(_items[index]);
				_items[index] = value;
				Register(_items[index]);

				Update();
			}
		}

		public void Add(TColumn item)
		{
			Register(item);

			_items.Add(item);

			Update();
		}

		public void Insert(int index, TColumn item)
		{
			Register(item);

			_items.Insert(index, item);

			Update();
		}

		public void Clear()
		{
			foreach (var item in _items)
				Unregister(item);

			_items.Clear();

			Update();
		}

		public bool Remove(TColumn item)
		{
			Unregister(item);

			var result = _items.Remove(item);

			Update();

			return result;
		}

		public void RemoveAt(int index)
		{
			Unregister(_items[index]);

			_items.RemoveAt(index);

			Update();
		}

		private void Register(TColumn column)
		{
			column.WidthChanged += OnColumnWidthChanged;
		}

		private void Unregister(TColumn column)
		{
			column.WidthChanged -= OnColumnWidthChanged;
		}

		private void Update()
		{

		}

		private void OnColumnWidthChanged(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		public int Count => _items.Count;
		public bool IsReadOnly => false;
		public bool Contains(TColumn item) => _items.Contains(item);
		public void CopyTo(TColumn[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
		public IEnumerator<TColumn> GetEnumerator() => _items.GetEnumerator();
		public int IndexOf(TColumn item) => _items.IndexOf(item);
		IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
	}
}
