using System.Collections.Generic;
using System.Linq;

namespace MzLite.Processing.Indexing
{

    public interface IBinarySearchComparer<TKey, TQuery>
    {
        int Compare(TKey key, TQuery query);
    }

    public sealed class IndexRange
    {

        private readonly int low;
        private readonly int heigh;

        internal IndexRange(int low, int heigh)
        {
            this.low = low;
            this.heigh = heigh;
        }

        public int Low { get { return low; } }
        public int Heigh { get { return heigh; } }
    }

    public abstract class IndexBase<TKey>
    {

        private readonly TKey[] keys;

        protected IndexBase(TKey[] keys)
        {
            this.keys = keys;
        }

        public TKey[] Keys { get { return keys; } }

        public IEnumerable<TKey> GetRange(IndexRange range)
        {
            for (int i = range.Low; i <= range.Heigh; i++)
            {
                yield return keys[i];
            }
        }

        public IEnumerable<TKey> BinarySearch<TQuery>(
            TQuery query,
            IBinarySearchComparer<TKey, TQuery> searchComparer)
        {
            IndexRange range;

            if (BinarySearch(query, searchComparer, out range))
            {
                return GetRange(range);
            }
            else
            {
                return Enumerable.Empty<TKey>();
            }
        }

        public bool BinarySearch<TQuery>(
            TQuery query,
            IBinarySearchComparer<TKey, TQuery> searchComparer,
            out IndexRange result)
        {

            result = null;

            int lo = 0;
            int hi = keys.Length - 1;

            while (lo <= hi)
            {
                int mid = lo + ((hi - lo) >> 1);

                int c = searchComparer.Compare(keys[mid], query);

                if (c == 0)
                {
                    int rangeLow = mid;
                    int rangeHeigh = mid;

                    for (int i = mid - 1; i >= 0; i--)
                    {
                        if (searchComparer.Compare(keys[i], query) == 0)
                            rangeLow = i;
                        else
                            break;
                    }

                    for (int i = mid + 1; i < keys.Length; i++)
                    {
                        if (searchComparer.Compare(keys[i], query) == 0)
                            rangeHeigh = i;
                        else
                            break;
                    }

                    result = new IndexRange(rangeLow, rangeHeigh);

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
}
