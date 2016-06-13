using System;
using System.Collections.Generic;
using System.Linq;

namespace MzLite.Processing.Indexing
{

    public interface IArrayIndexKey
    {
        int SourceIndex { get; }
    }

    public abstract class ArrayIndexKey : IArrayIndexKey
    {

        private readonly int sourceIndex;

        protected ArrayIndexKey(int sourceIndex)
        {
            this.sourceIndex = sourceIndex;
        }

        public int SourceIndex { get { return sourceIndex; } }
    }

    public sealed class ArrayIndex<TKey> : IndexBase<TKey>
        where TKey : IArrayIndexKey
    {

        public ArrayIndex(TKey[] keys) 
            : base(keys) { }

        public IEnumerable<TSource> GetRange<TSource>(TSource[] source, IndexRange range)
        {
            for (int i = range.Low; i <= range.Heigh; i++)
            {
                yield return source[Keys[i].SourceIndex];
            }
        }

        public IEnumerable<TSource> BinarySearch<TSource,TQuery>(
            TSource[] source, 
            TQuery query, 
            IBinarySearchComparer<TKey, TQuery> comparer)
        {
            IndexRange range;

            if (BinarySearch(query, comparer, out range))
            {
                return GetRange(source, range);
            }
            else
            {
                return Enumerable.Empty<TSource>();
            }
        }
    }

    public static class ArrayIndex
    {

        public static ArrayIndex<TKey> BuildIndex<TSource, TKey>(
            TSource[] source,            
            Func<TSource, int, TKey> keyfun,
            bool sortKeys)
            where TKey : IArrayIndexKey, IComparable<TKey>
        {

            TKey[] keys = new TKey[source.Length];
            for (int i = 0; i < source.Length; i++)
                keys[i] = keyfun.Invoke(source[i], i);

            if (sortKeys)
                Array.Sort(keys);

            return new ArrayIndex<TKey>(keys);
        }

    }
}
