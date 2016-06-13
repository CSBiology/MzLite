using System;
using System.Collections.Generic;
using System.Linq;

namespace MzLite.Processing.Indexing
{

    public interface IIDIndexKey<TID>
    {
        TID SourceID { get; }
    }

    public abstract class IDIndexKey<TID> : IIDIndexKey<TID>
    {

        private readonly TID sourceID;

        protected IDIndexKey(TID sourceID)
        {
            this.sourceID = sourceID;
        }

        public TID SourceID { get { return sourceID; } }
    }

    public sealed class IDIndex<TID, TKey> : IndexBase<TKey>
        where TKey : IIDIndexKey<TID>
    {

        public IDIndex(TKey[] keys)
            : base(keys) { }

        public IEnumerable<TSource> GetRange<TSource>(Func<TID, TSource> sourceAccessor, IndexRange range)
        {
            for (int i = range.Low; i <= range.Heigh; i++)
            {
                yield return sourceAccessor.Invoke(Keys[i].SourceID);
            }
        }

        public IEnumerable<TSource> BinarySearch<TSource, TQuery>(
            Func<TID, TSource> sourceAccessor,
            TQuery query, 
            IBinarySearchComparer<TKey, TQuery> searchComparer)
        {
            IndexRange range;

            if (BinarySearch(query, searchComparer, out range))
            {
                return GetRange(sourceAccessor, range);
            }
            else
            {
                return Enumerable.Empty<TSource>();
            }
        }
    }

    public static class IDIndex
    {

        public static IDIndex<TID, TKey> BuildIndex<TSource, TID, TKey>(
            IEnumerable<TSource> source,
            Func<IEnumerable<TSource>, IEnumerable<TKey>> keyfun,
            bool sortKeys)
            where TKey : IIDIndexKey<TID>, IComparable<TKey>
        {

            IList<TKey> list = new List<TKey>();

            foreach (var k in keyfun.Invoke(source))
                list.Add(k);

            TKey[] keys = list.ToArray();

            if (sortKeys)
                Array.Sort(keys);

            return new IDIndex<TID, TKey>(keys);
        }

        public static IDIndex<TID, TKey> BuildIndex<TSource, TID, TKey>(
            IEnumerable<TSource> source,
            Func<TSource, TKey> keyfun,
            bool sortKeys)
            where TKey : IIDIndexKey<TID>, IComparable<TKey>
        {

            IList<TKey> list = new List<TKey>();
            
            foreach (var s in source)
                list.Add(keyfun.Invoke(s));

            TKey[] keys = list.ToArray();

            if (sortKeys)
                Array.Sort(keys);

            return new IDIndex<TID, TKey>(keys);
        }
    }
}
