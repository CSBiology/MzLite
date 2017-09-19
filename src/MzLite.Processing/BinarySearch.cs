#region license
// The MIT License (MIT)

// BinarySearch.cs

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
using MzLite.Commons.Arrays;

namespace MzLite.Processing
{
    public static class BinarySearch
    {

        public static IEnumerable<TItem> Search<TItem, TQuery>(
            TItem[] items,
            TQuery query,
            Func<TItem, TQuery, int> searchCompare)
        {
            return Search<TItem, TQuery>(items.ToMzLiteArray(), query, searchCompare);
        }

        public static IEnumerable<TItem> Search<TItem, TQuery>(
            IMzLiteArray<TItem> items,
            TQuery query,
            Func<TItem, TQuery, int> searchCompare)
        {
            IndexRange result;

            if (Search(items, query, searchCompare, out result))
            {
                return IndexRange.EnumRange(items, result);
            }
            else
            {
                return Enumerable.Empty<TItem>();
            }
        }

        public static bool Search<TItem, TQuery>(
            TItem[] items,
            TQuery query,
            Func<TItem, TQuery, int> searchCompare,
            out IndexRange result)
        {

            return Search<TItem, TQuery>(items.ToMzLiteArray(), query, searchCompare, out result);
        }

        public static bool Search<TItem, TQuery>(
            IMzLiteArray<TItem> items,
            TQuery query,
            Func<TItem, TQuery, int> searchCompare,
            out IndexRange result)
        {            

            int lo = 0;
            int hi = items.Length - 1;

            while (lo <= hi)
            {
                int mid = lo + ((hi - lo) >> 1);

                int c = searchCompare(items[mid], query);

                if (c == 0)
                {
                    
                    int resultLow = mid;
                    int resultHeigh = mid;

                    // search tees low
                    for (int i = mid - 1; i >= 0; i--)
                    {
                        if (searchCompare(items[i], query) == 0)
                            resultLow = i;
                        else
                            break;
                    }

                    // search tees heigh
                    for (int i = mid + 1; i < items.Length; i++)
                    {
                        if (searchCompare(items[i], query) == 0)
                            resultHeigh = i;
                        else
                            break;
                    }

                    result = new IndexRange(resultLow, resultHeigh);
                    return true;
                }

                if (c < 0)
                {
                    lo = mid + 1;
                }
                else
                {
                    hi = mid - 1;
                }
            }

            result = null;
            return false;
        }

    }

    public sealed class IndexRange
    {

        private readonly int low;
        private readonly int heigh;

        internal IndexRange(int low, int heigh)
        {
            if (low < 0 || heigh < 0)
                throw new ArgumentOutOfRangeException("low or heigh may not be < 0.");
            if(heigh < low)
                throw new ArgumentOutOfRangeException("low <= heigh expected.");
            this.low = low;
            this.heigh = heigh;
        }

        public int Low { get { return low; } }
        public int Heigh { get { return heigh; } }
        public int Length { get { return (heigh - low) + 1; } }

        /// <summary>
        /// Maps index to source array index.
        /// </summary>
        /// <param name="i"></param>
        /// <returns>Low + i</returns>
        public int GetSourceIndex(int i)
        {
            if (i < 0)
                throw new ArgumentOutOfRangeException("i >= 0 expected.");
            return Low + i;
        }

        public static IEnumerable<TItem> EnumRange<TItem>(TItem[] items, IndexRange range)
        {
            return EnumRange<TItem>(items.ToMzLiteArray(), range);
        }

        public static IEnumerable<TItem> EnumRange<TItem>(IMzLiteArray<TItem> items, IndexRange range)
        {
            for (int i = range.Low; i <= range.Heigh; i++)
            {
                yield return items[i];
            }
        }        
    }
}
