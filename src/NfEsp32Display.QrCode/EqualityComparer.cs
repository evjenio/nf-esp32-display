namespace System.Collections.Generic
{
    // The generic IEqualityComparer interface implements methods to if check two objects are equal
    // and generate Hashcode for an object.
    // It is use in Dictionary class.  
    public interface IEqualityComparer<in T>
    {
        bool Equals(T x, T y);
        int GetHashCode(T obj);
    }
    [Serializable]
    public abstract class EqualityComparer<T> : IEqualityComparer, IEqualityComparer<T>
    {
        static readonly EqualityComparer<T> defaultComparer = null;

        public static EqualityComparer<T> Default
        {
            get
            {
                return defaultComparer;
            }
        }



        public abstract bool Equals(T x, T y);

        public abstract int GetHashCode(T obj);

        internal virtual int IndexOf(T[] array, T value, int startIndex, int count)
        {
            int endIndex = startIndex + count;
            for (int i = startIndex; i < endIndex; i++)
            {
                if (Equals(array[i], value)) return i;
            }
            return -1;
        }

        internal virtual int LastIndexOf(T[] array, T value, int startIndex, int count)
        {
            int endIndex = startIndex - count + 1;
            for (int i = startIndex; i >= endIndex; i--)
            {
                if (Equals(array[i], value)) return i;
            }
            return -1;
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            if (obj == null) return 0;
            if (obj is T) return GetHashCode((T)obj);
            return 0;
        }

        bool IEqualityComparer.Equals(object x, object y)
        {
            if (x == y) return true;
            if (x == null || y == null) return false;
            if ((x is T) && (y is T)) return Equals((T)x, (T)y);
            return false;
        }
    }
}