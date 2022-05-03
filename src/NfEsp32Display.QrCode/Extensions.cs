using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace System.Linq
{
    public static class Extensions
    {

        public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                list.Add(item);
            }
        }

        public static void AddRange<T>(this IList<T> list, T[] items)
        {
            foreach (var item in items)
            {
                list.Add(item);
            }
        }

        public static T Single<T>(this IEnumerable<T> list, Func<T, bool> predicate)
        {
            foreach (var item in list)
            {
                if (predicate(item))
                {
                    return item;
                }
            }
            throw new InvalidOperationException();
        }

        public static T Single<T>(this T[] list, Func<T, bool> predicate)
        {
            foreach (var item in list)
            {
                if (predicate(item))
                {
                    return item;
                }
            }
            throw new InvalidOperationException();
        }

        public static void RemoveAll<T>(this IList<T> list, Func<T, bool> predicate)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if (predicate(item))
                {
                    list.Remove(item);
                    i--;
                }
            }
        }

        public static void SortByDescending<T>(this IList<T> list, Func<T, int> keySelector)
        {
            T temp;
            for (int i = 0; i < list.Count - 1; i++)
            {
                for (int j = 0; j < list.Count - 1 - i; j++)
                {
                    if (keySelector(list[j]) < keySelector(list[j + 1]))
                    {
                        temp = list[j];
                        list[j] = list[j + 1];
                        list[j + 1] = temp;
                    }
                }
            }
        }

        public static IEnumerable<T> Where<T>(this IEnumerable<T> list, Func<T, bool> predicate)
        {
            var result = new List<T>();
            foreach (var item in list)
            {
                if (predicate(item))
                {
                    result.Add(item);
                }
            }
            return result;
        }

        public static IEnumerable<T2> Select<T1, T2>(this IEnumerable<T1> list, Func<T1, T2> selector)
        {
            var result = new List<T2>();
            foreach (var item in list)
            {
                result.Add(selector(item));
            }
            return result;
        }

        public static IEnumerable<T2> Select<T1, T2>(this IEnumerable<T1> list, Func<T1, int, T2> selector)
        {
            var result = new List<T2>();
            int i = 0;
            foreach (var item in list)
            {
                result.Add(selector(item, i++));
            }
            return result;
        }

        public static T First<T>(this IEnumerable<T> list, Func<T, bool> predicate) => Single(list, predicate);
        public static T First<T>(this T[] list, Func<T, bool> predicate) => Single(list, predicate);
        public static T First<T>(this IEnumerable<T> list) => Single<T>(list, _ => true);

        public static int Count<T>(this IEnumerable<T> source)
        {
            int count = 0;
            using (IEnumerator<T> e = source.GetEnumerator())
            {
                checked
                {
                    while (e.MoveNext()) count++;
                }
            }
            return count;
        }

        public static int Count<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            int count = 0;
            foreach (var item in source)
            {
                if (predicate(item)) count++;
            }
            return count;
        }

        public static int Min<T>(this IEnumerable<T> source, Func<T, int> selector)
        {
            int min = int.MaxValue;
            foreach (var item in source)
            {
                var key = selector(item);
                if (key < min)
                {
                    min = key;
                }
            }
            return min;
        }

        public static T[] ToArray<T>(this IList<T> list)
        {
            var array = new T[list.Count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = list[i];
            }
            return array;
        }

        public static T[] ToArray<T>(this IEnumerable<T> list)
        {
            var array = new T[list.Count()];
            int i = 0;
            foreach (var item in list)
            {
                array[i++] = item;
            }
            return array;
        }

        public static List<T> ToList<T>(this IEnumerable<T> list)
        {
            var array = new List<T>();
            foreach (var item in list)
            {
                array.Add(item);
            }
            return array;
        }

        public static IEnumerable<T> AsEnumerable<T>(this IEnumerable list)
        {
            var result = new List<T>();
            foreach (T item in list)
            {
                result.Add(item);
            }
            return result;
        }

        public static IEnumerable<IGrouping<TKey, TValue>> GroupBy<TKey,TValue>(this IEnumerable<TValue> list, Func<TValue, TKey> selector)
        {
            var result = new Hashtable();
            foreach(var item in list)
            {
                var key = selector(item);
                var values = result[key] as List<TValue>;
                if(values == default)
                {
                    values = new List<TValue>()
                    {
                        item
                    };
                    result.Add(key, values);
                }
                else
                {
                    values.Add(item);
                    result[key] = values;
                }
            }

            var groups = new List<Grouping<TKey,TValue>>();
            foreach (DictionaryEntry item in result)
            {
                groups.Add(new Grouping<TKey, TValue>((TKey)item.Key, (IEnumerable<TValue>)item.Value));
            }
            return groups;
        }

        public static TAccumulate Aggregate<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
        {
            TAccumulate result = seed;
            foreach (TSource element in source) result = func(result, element);
            return result;
        }

        public static string Remove(this string text, int startIndex, int length)
        {
            var arr = text.ToCharArray();
            var dstLength = arr.Length - length;
            var dst = new char[dstLength];
            Array.Copy(arr, startIndex, dst, 0, dstLength);
            return new string(dst);
        }

        public static string Join(this IEnumerable<string> strings)
        {
            var sb = new StringBuilder();
            foreach (var item in strings)
            {
                sb.Append(item);
            }
            return sb.ToString();
        }
    }

    public interface IGrouping<out TKey, out TElement> : IEnumerable<TElement>
    {
        TKey Key { get; }
    }

    public class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        private readonly TKey key;
        private readonly IEnumerable<TElement> values;

        public Grouping(TKey key, IEnumerable<TElement> values)
        {
            this.key = key;
            this.values = values;
        }

        public TKey Key => key;

        public IEnumerator<TElement> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
