using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProceduralGraph;

internal sealed class ObservableCollection<T> : IList, IList<T>, ICloneable, INotifyCollectionChanged, INotifyPropertyChanged where T : class, INotifyPropertyChanged
{
    public struct Enumerator : IEnumerator<T>
    {
        private readonly ObservableCollection<T> _collection;
        private int _index;
        private T? _current;

        internal Enumerator(ObservableCollection<T> collection)
        {
            _collection = collection;
            _index = 0;
            _current = default;
        }

        public readonly T Current => _current!;
        readonly object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if ((uint)_index < (uint)_collection.Count)
            {
                _current = _collection._items[_index];
                _index++;
                return true;
            }
            _index = _collection.Count + 1;
            _current = default;
            return false;
        }

        public void Reset()
        {
            _index = 0;
            _current = default;
        }

        public readonly void Dispose() { }
    }

    private const int DefaultCapacity = 4;
    private const string IndexerName = "Item[]";

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly PropertyChangedEventHandler _itemPropertyChangedHandler;
    public event PropertyChangedEventHandler? ItemPropertyChanged;

    private T[] _items;

    public int Count { get; private set; }

    bool IList.IsReadOnly => false;
    bool ICollection<T>.IsReadOnly => false;

    bool IList.IsFixedSize => false;

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot { get; } = new object();

    public ObservableCollection()
    {
        _items = GC.AllocateUninitializedArray<T>(DefaultCapacity);
        _itemPropertyChangedHandler = OnItemPropertyChanged;
    }

    public ObservableCollection(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity, nameof(capacity));
        _items = GC.AllocateUninitializedArray<T>(capacity);
        _itemPropertyChangedHandler = OnItemPropertyChanged;
    }

    public ObservableCollection(IEnumerable<T> collection) : this()
    {
        ArgumentNullException.ThrowIfNull(collection);
        foreach (var item in collection)
        {
            Add(item);
        }
    }

    public T this[int index]
    {
        get
        {
            if ((uint)index >= (uint)Count) throw new ArgumentOutOfRangeException(nameof(index));
            return _items[index];
        }
        set
        {
            if ((uint)index >= (uint)Count) throw new ArgumentOutOfRangeException(nameof(index));
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            T originalItem = _items[index];

            if (ReferenceEquals(originalItem, value))
            {
                return;
            }

            if (originalItem != null)
            {
                originalItem.PropertyChanged -= _itemPropertyChangedHandler;
            }
            value.PropertyChanged += _itemPropertyChangedHandler;

            _items[index] = value;

            OnCollectionChanged(NotifyCollectionChangedAction.Replace, value, originalItem, index);
            OnPropertyChanged(IndexerName);
        }
    }

#nullable disable
    object IList.this[int index]
    {
        get => this[index];
        set => this[index] = (T)value;
    }
#nullable enable

    public void Add(T item)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));

        if (Count >= _items.Length)
        {
            EnsureCapacity(Count + 1);
        }

        item.PropertyChanged += _itemPropertyChangedHandler;

        int index = Count++;
        _items[index] = item;

        OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged(IndexerName);
    }

    public int Add(object? value)
    {
        if (value is T item)
        {
            Add(item);
            return Count - 1;
        }

        throw new ArgumentException($"Value must be of type {typeof(T).Name}", nameof(value));
    }

    public void Insert(int index, T item)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));
        ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)index, (uint)Count, nameof(index));

        if (Count == _items.Length)
        {
            EnsureCapacity(Count + 1);
        }

        if (index < Count)
        {
            Array.Copy(_items, index, _items, index + 1, Count - index);
        }

        item.PropertyChanged += _itemPropertyChangedHandler;

        _items[index] = item;
        Count++;

        OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged(IndexerName);
    }

    public void Insert(int index, object? value)
    {
        if (value is T item)
        {
            Insert(index, item);
            return;
        }
        
        throw new ArgumentException($"Value must be of type {typeof(T).Name}", nameof(value));
    }

    public bool Remove(T item)
    {
        int index = Array.IndexOf(_items, item, 0, Count);
        if (index < 0)
        {
            return false;
        }

        RemoveAt(index);
        return true;
    }

    public void Remove(object? value)
    {
        if (value is T item)
        {
            Remove(item);
        }
    }

    public void RemoveAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Count, nameof(index));

        T item = _items[index];

        if (item is not null)
        {
            item.PropertyChanged -= _itemPropertyChangedHandler;
        }

        Count--;
        if (index < Count)
        {
            Array.Copy(_items, index + 1, _items, index, Count - index);
        }

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            _items[Count] = default!;
        }

        OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged(IndexerName);
    }

    public void Clear()
    {
        if (Count == 0) return;

        for (int i = 0; i < Count; i++)
        {
            if (_items[i] is null)
            {
                continue;
            }

            _items[i].PropertyChanged -= _itemPropertyChangedHandler;
        }

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            Array.Clear(_items, 0, Count);
        }

        Count = 0;

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged(IndexerName);
    }

    /// <summary>
    /// Resizes the collection to a specific size. 
    /// If shrinking, items outside the new bounds are removed and unsubscribed.
    /// If growing, new slots are initialized to default (null) but added to Count.
    /// </summary>
    public void Resize(int newSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(newSize);

        if (newSize == Count)
        {
            return;
        }

        if (newSize < Count)
        {
            for (int i = newSize; i < Count; i++)
            {
                if (_items[i] is null)
                {
                    continue;
                }

                _items[i].PropertyChanged -= _itemPropertyChangedHandler;
            }

            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                Array.Clear(_items, newSize, Count - newSize);
            }

            Count = newSize;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(IndexerName);
        }
        else
        {
            EnsureCapacity(newSize);
            Count = newSize;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(IndexerName);
        }
    }

    /// <summary>
    /// Creates a new <see cref="ObservableCollection{T}"/> that is a copy of the current instance.
    /// </summary>
    /// <returns>A new object that is a copy of this instance.</returns>
    public ObservableCollection<T> Clone()
    {
        var clone = new ObservableCollection<T>(Count);

        for (int i = 0; i < Count; i++)
        {
            if (_items[i] is null)
            {
                continue;
            }

            clone.Add(_items[i]);
        }

        return clone;
    }
    object ICloneable.Clone() => Clone();

    public bool Contains(T item) => Array.IndexOf(_items, item, 0, Count) != -1;
    public bool Contains(object? value) => value is T item && Contains(item);

    public int IndexOf(T item) => Array.IndexOf(_items, item, 0, Count);
    public int IndexOf(object? value) => value is T item ? IndexOf(item) : -1;

    public void CopyTo(T[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        new ReadOnlySpan<T>(_items, 0, Count).CopyTo(array.AsSpan(arrayIndex));
    }

    public void CopyTo(Array array, int index)
    {
        ArgumentNullException.ThrowIfNull(array);
        Array.Copy(_items, 0, array, index, Count);
    }

    public Enumerator GetEnumerator() => new(this);
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);
    IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        ItemPropertyChanged?.Invoke(sender, e);
    }

    private void OnCollectionChanged(NotifyCollectionChangedAction action, object? item, int index)
    {
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, item, index));
    }

    private void OnCollectionChanged(NotifyCollectionChangedAction action, object? newItem, object? oldItem, int index)
    {
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
    }

    private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void EnsureCapacity(int min)
    {
        if (_items.Length >= min)
        { 
            return; 
        }

        int newCapacity = _items.Length == 0 ? DefaultCapacity : _items.Length * 2;

        if ((uint)newCapacity > Array.MaxLength) 
        { 
            newCapacity = Array.MaxLength; 
        }

        if (newCapacity < min) 
        { 
            newCapacity = min; 
        }

        Array.Resize(ref _items, newCapacity);
    }
}