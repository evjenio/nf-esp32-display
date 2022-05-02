namespace System.Collections.Generic
{
    public interface ICollection<T> : IEnumerable<T>
    {
        // Number of items in the collections.        
        int Count { get; }

        bool IsReadOnly { get; }

        void Add(T item);

        void Clear();

        bool Contains(T item);

        bool Remove(T item);
    }

    public interface IList<T> : ICollection<T>
    {
        // The Item property provides methods to read and edit entries in the List.
        T this[int index]
        {
            get;
            set;
        }

        // Returns the index of a particular item, if it is in the list.
        // Returns -1 if the item isn't in the list.
        int IndexOf(T item);

        // Inserts value into the list at position index.
        // index must be non-negative and less than or equal to the 
        // number of elements in the list.  If index equals the number
        // of items in the list, then value is appended to the end.
        void Insert(int index, T item);

        // Removes the item at position index.
        void RemoveAt(int index);
    }

    public interface IEnumerator<out T> : IDisposable, IEnumerator
    {
        new T Current
        {
            get;
        }
    }

    public interface IEnumerable<out T> : IEnumerable
    {
        new IEnumerator<T> GetEnumerator();
    }

    public class List<T> : IList<T>
    {
        private readonly ArrayList _array;

        public List()
        {
            _array = new ArrayList();
        }

        public List(T[] items)
        {
            _array = new ArrayList();
            foreach (var item in items)
            {
                _array.Add(item);
            }
        }

        public T this[int index]
        {
            get => (T)_array[index];
            set => _array[index] = value;
        }

        public int Count => _array.Count;

        public bool IsReadOnly => _array.IsReadOnly;

        public void Add(T item)
        {
            _array.Add(item);
        }

        public void Clear()
        {
            _array.Clear();
        }

        public bool Contains(T item) => _array.Contains(item);

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        public int IndexOf(T item)
        {
            return _array.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _array.Insert(index, item);
        }

        public bool Remove(T item)
        {
            var count = _array.Count;
            _array.Remove(item);
            return count != _array.Count;
        }

        public void RemoveAt(int index)
        {
            _array.RemoveAt(index);
        }

        public int Length => _array.Count;

        IEnumerator IEnumerable.GetEnumerator() => _array.GetEnumerator();

        public struct Enumerator : IEnumerator<T>
        {
            private readonly IEnumerator _enumerator;

            public Enumerator(IList<T> list)
            {
                _enumerator = (list as IEnumerable).GetEnumerator();
            }

            public T Current => (T)_enumerator.Current;

            object IEnumerator.Current => _enumerator.Current;

            public void Dispose()
            {
            }

            public bool MoveNext() => _enumerator.MoveNext();

            public void Reset() => _enumerator.Reset();
        }
    }
}
