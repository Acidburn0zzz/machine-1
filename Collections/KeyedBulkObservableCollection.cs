﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SIL.Collections
{
	public class KeyedBulkObservableCollection<TKey, TItem> : BulkObservableCollection<TItem>, IKeyedCollection<TKey, TItem>
	{
		private readonly Func<TItem, TKey> _getKeyForItem;
		private readonly IEqualityComparer<TKey> _comparer;
		private Dictionary<TKey, TItem> _dict;
		private int _keyCount;
		private readonly int _threshold;

		public KeyedBulkObservableCollection(Func<TItem, TKey> getKeyForItem)
			: this(Enumerable.Empty<TItem>(), getKeyForItem, null)
		{
		}

		public KeyedBulkObservableCollection(Func<TItem, TKey> getKeyForItem, IEqualityComparer<TKey> comparer)
			: this(Enumerable.Empty<TItem>(), getKeyForItem, comparer, 0)
		{
		}

		public KeyedBulkObservableCollection(Func<TItem, TKey> getKeyForItem, IEqualityComparer<TKey> comparer, int dictionaryCreationThreshold)
			: this(Enumerable.Empty<TItem>(), getKeyForItem, comparer, dictionaryCreationThreshold)
		{
		}

		public KeyedBulkObservableCollection(IEnumerable<TItem> items, Func<TItem, TKey> getKeyForItem)
			: this(items, getKeyForItem, null)
		{
		}

		public KeyedBulkObservableCollection(IEnumerable<TItem> items, Func<TItem, TKey> getKeyForItem, IEqualityComparer<TKey> comparer)
			: this(items, getKeyForItem, comparer, 0)
		{
		}

		public KeyedBulkObservableCollection(IEnumerable<TItem> items, Func<TItem, TKey> getKeyForItem, IEqualityComparer<TKey> comparer, int dictionaryCreationThreshold)
		{
			_getKeyForItem = getKeyForItem;

			if (comparer == null)
				comparer = EqualityComparer<TKey>.Default;

			if (dictionaryCreationThreshold == -1)
				dictionaryCreationThreshold = 2147483647;

			if (dictionaryCreationThreshold < -1)
				throw new ArgumentOutOfRangeException("dictionaryCreationThreshold");

			_comparer = comparer;
			_threshold = dictionaryCreationThreshold;
			AddRange(items);
		}

		protected KeyedBulkObservableCollection()
			: this((IEqualityComparer<TKey>) null)
		{
		}

		protected KeyedBulkObservableCollection(IEqualityComparer<TKey> comparer)
			: this(comparer, 0)
		{
		}

		protected KeyedBulkObservableCollection(IEqualityComparer<TKey> comparer, int dictionaryCreationThreshold)
			: this(null, comparer, dictionaryCreationThreshold)
		{
		}

		public IEqualityComparer<TKey> Comparer
		{
			get { return _comparer; }
		}

		protected IDictionary<TKey, TItem> Dictionary
		{
			get { return _dict; }
		}

		public virtual TItem this[TKey key]
		{
			get
			{
				if (key == null)
					throw new ArgumentNullException("key");

				if (_dict != null)
					return _dict[key];

				foreach (TItem current in Items)
				{
					if (_comparer.Equals(GetKeyForItem(current), key))
						return current;
				}
				throw new KeyNotFoundException();
			}
		}

		public bool TryGetValue(TKey key, out TItem value)
		{
			if (_dict != null)
				return _dict.TryGetValue(key, out value);
			foreach (TItem current in Items)
			{
				if (_comparer.Equals(GetKeyForItem(current), key))
				{
					value = current;
					return true;
				}
			}
			value = default(TItem);
			return false;
		}

		public bool Contains(TKey key)
		{
			if (key == null)
				throw new ArgumentNullException("key");

			if (_dict != null)
				return _dict.ContainsKey(key);

			foreach (TItem current in Items)
			{
				if (_comparer.Equals(GetKeyForItem(current), key))
					return true;
			}
			return false;
		}

		public bool Remove(TKey key)
		{
			if (key == null)
				throw new ArgumentNullException("key");

			if (_dict != null)
				return _dict.ContainsKey(key) && Remove(_dict[key]);

			for (int i = 0; i < Items.Count; i++)
			{
				if (_comparer.Equals(GetKeyForItem(Items[i]), key))
				{
					RemoveItem(i);
					return true;
				}
			}
			return false;
		}

		protected virtual TKey GetKeyForItem(TItem item)
		{
			return _getKeyForItem(item);
		}

		protected override void InsertItem(int index, TItem item)
		{
			TKey key = GetKeyForItem(item);
			if (key != null)
				AddKey(key, item);

			base.InsertItem(index, item);
		}

		protected override void ClearItems()
		{
			base.ClearItems();
			if (_dict != null)
				_dict.Clear();
			_keyCount = 0;
		}

		protected override void RemoveItem(int index)
		{
			TKey key = GetKeyForItem(Items[index]);
			if (key != null)
			{
				RemoveKey(key);
			}
			base.RemoveItem(index);
		}

		protected override void SetItem(int index, TItem item)
		{
			TKey key = GetKeyForItem(item);
			TKey key2 = GetKeyForItem(Items[index]);
			if (_comparer.Equals(key2, key))
			{
				if (key != null && _dict != null)
					_dict[key] = item;
			}
			else
			{
				if (key != null)
					AddKey(key, item);

				if (key2 != null)
					RemoveKey(key2);
			}
			base.SetItem(index, item);
		}

		private void AddKey(TKey key, TItem item)
		{
			if (_dict != null)
			{
				_dict.Add(key, item);
				return;
			}

			if (_keyCount == _threshold)
			{
				CreateDictionary();
				Debug.Assert(_dict != null);
				_dict.Add(key, item);
				return;
			}

			if (Contains(key))
				throw new ArgumentException("The collection cannot contain duplicate keys.");

			_keyCount++;
		}

		private void RemoveKey(TKey key)
		{
			if (_dict != null)
			{
				_dict.Remove(key);
				return;
			}
			_keyCount--;
		}

		private void CreateDictionary()
		{
			_dict = new Dictionary<TKey, TItem>(_comparer);
			foreach (TItem current in Items)
			{
				TKey key = GetKeyForItem(current);
				if (key != null)
					_dict.Add(key, current);
			}
		}
	}
}