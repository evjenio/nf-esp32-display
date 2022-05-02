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

        public static IEnumerable<T2> Select<T1,T2>(this IEnumerable<T1> list, Func<T1, T2> selector)
        {
            var result = new List<T2>();
            foreach (var item in list)
            {
                result.Add(selector(item));
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

        public static string Remove(this string text, int startIndex, int length)
        {
            var arr = text.ToCharArray();
            var dstLength = arr.Length - length;
            var dst = new char[dstLength];
            Array.Copy(arr, startIndex, dst, 0, dstLength);
            return new string(dst);
        }
    }
}
