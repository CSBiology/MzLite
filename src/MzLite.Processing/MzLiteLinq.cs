#region license
// The MIT License (MIT)

// MzLiteLinq.cs

// Copyright (c) 2016 Alexander Lüdemann
// alexander.luedemann@outlook.com
// luedeman@rhrk.uni-kl.de

// Computational Systems Biology, Technical University of Kaiserslautern, Germany
 

// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

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
