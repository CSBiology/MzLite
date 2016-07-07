#region license
// The MIT License (MIT)

// MzLiteArray.cs

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
using System.Linq;

namespace MzLite.Commons.Arrays
{

    /// <summary>
    /// Defines a simple one dimensional interface for array-like data structures.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMzLiteArray<T> : IEnumerable<T>
    {        
        int Length { get; }
        T this[int idx] { get; }
    }

    public static class MzLiteArray
    {

        public static IMzLiteArray<T> ToMzLiteArray<T>(this T[] array)
        {
            return new ArrayWrapper<T>(array);
        }

        public static T[] ToCLRArray<T>(this IMzLiteArray<T> array)
        {
            T[] copy = new T[array.Length];
            for (int i = 0; i < array.Length; i++)
                copy[i] = array[i];
            return copy;
        }

        public static IMzLiteArray<T> Empty<T>()
        {
            return new ArrayWrapper<T>(new T[0]);
        }
    }

    internal class ArrayWrapper<T> : IMzLiteArray<T>
    {

        private readonly T[] array;

        internal ArrayWrapper(T[] array)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (array.Rank != 1)
                throw new RankException("One dim array expected.");
            this.array = array;
        }

        #region IMzLiteArray<T> Members
        
        public int Length
        {
            get { return array.Length; }
        }

        public T this[int idx]
        {
            get { return array[idx]; }
        }
        
        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return array.AsEnumerable<T>().GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return array.GetEnumerator();
        }

        #endregion
    }

}
