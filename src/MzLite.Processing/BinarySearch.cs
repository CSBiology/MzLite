using System;
using System.Collections.Generic;
using System.Linq;

namespace MzLite.Processing
{
    public static class BinarySearch
    {

        public static IEnumerable<TItem> Search<TItem, TQuery>(
            TItem[] items,
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

            result = new IndexRange(-1, -1);

            int lo = 0;
            int hi = items.Length - 1;

            while (lo <= hi)
            {
                int mid = lo + ((hi - lo) >> 1);

                int c = searchCompare(items[mid], query);

                if (c == 0)
                {

                    result.Low = mid;
                    result.Heigh = mid;

                    // search tees low
                    for (int i = mid - 1; i >= 0; i--)
                    {
                        if (searchCompare(items[i], query) == 0)
                            result.Low = i;
                        else
                            break;
                    }

                    // search tees heigh
                    for (int i = mid + 1; i < items.Length; i++)
                    {
                        if (searchCompare(items[i], query) == 0)
                            result.Heigh = i;
                        else
                            break;
                    }

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

            return false;
        }

    }

    public sealed class IndexRange
    {
        public IndexRange(int low, int heigh)
        {
            this.Low = low;
            this.Heigh = heigh;
        }

        public int Low { get; internal set; }
        public int Heigh { get; internal set; }

        public static IEnumerable<TItem> EnumRange<TItem>(TItem[] items, IndexRange range)
        {
            for (int i = range.Low; i <= range.Heigh; i++)
            {
                yield return items[i];
            }
        }

        public static IEnumerable<TSource> EnumRange<TSource, TItem>(TSource[] source, TItem[] items, Func<TItem, int> mapIndex, IndexRange range)
        {
            for (int i = range.Low; i <= range.Heigh; i++)
            {
                yield return source[mapIndex.Invoke(items[i])];
            }
        }
    }
}
