using System;
using System.Collections.Generic;

namespace MzLite.Processing
{
    public static class MzLiteLinq
    {

        public static T ItemAtMin<T, TValue>(this IEnumerable<T> source, Func<T, TValue> valFunc)
            where TValue : IComparable<TValue>
        {

            var enumerator = source.GetEnumerator();

            if (!enumerator.MoveNext())
                throw new InvalidOperationException("Sequence contains no elements.");

            T min = enumerator.Current;

            while (enumerator.MoveNext())
            {
                T item = enumerator.Current;
                TValue val = valFunc.Invoke(item);

                if (val.CompareTo(valFunc.Invoke(min)) < 0)
                    min = item;
            }

            return min;
        }        

        public static T ItemAtMax<T, TValue>(this IEnumerable<T> source, Func<T, TValue> valFunc)
            where TValue : IComparable<TValue>
        {

            var enumerator = source.GetEnumerator();

            if (!enumerator.MoveNext())
                throw new InvalidOperationException("Sequence contains no elements.");

            T max = enumerator.Current;

            while (enumerator.MoveNext())
            {
                T item = enumerator.Current;
                TValue val = valFunc.Invoke(item);

                if (val.CompareTo(valFunc.Invoke(max)) > 0)
                    max = item;
            }

            return max;
        }        
    }
}
