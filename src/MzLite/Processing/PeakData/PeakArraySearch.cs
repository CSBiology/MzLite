using System;
using System.Collections.Generic;
using MzLite.Binary;
using MzLite.Processing.Indexing;

namespace MzLite.Processing.PeakData
{

    public sealed class MzRangeQuery
    {
        private readonly double lowMz;
        private readonly double highMz;

        public MzRangeQuery(double low, double high)
        {
            this.lowMz = low;
            this.highMz = high;
        }

        public MzRangeQuery(
            double mz,
            double mzOffsetLow,
            double mzOffsetHeigh)
            : this(DecimalHelper.Subtract(mz, mzOffsetLow),
                DecimalHelper.Add(mz, mzOffsetHeigh)) { }

        public double LowMz { get { return lowMz; } }
        public double HighMz { get { return highMz; } }
    }

    public sealed class PeakMzKey : ArrayIndexKey, IComparable<PeakMzKey>
    {

        private readonly double mz;

        private PeakMzKey(double mz, int index)
            : base(index)
        {
            this.mz = mz;
        }

        public double Mz { get { return mz; } }

        public static PeakMzKey CreateKey(Peak1D peak, int index)
        {
            return new PeakMzKey(peak.Mz, index);
        }

        public static PeakMzKey CreateKey(Peak2D peak, int index)
        {
            return new PeakMzKey(peak.Mz, index);
        }

        #region IComparable<PeakMzKey> Members

        public int CompareTo(PeakMzKey other)
        {
            return mz.CompareTo(other.mz);
        }

        #endregion
    }

    public sealed class PeakArrayMzIndexComparer : IBinarySearchComparer<PeakMzKey, MzRangeQuery>
    {

        internal PeakArrayMzIndexComparer() { }

        #region IBinarySearchComparer<PeakMzKey,MzRangeQuery> Members

        public int Compare(PeakMzKey key, MzRangeQuery query)
        {
            if (key.Mz < query.LowMz)
                return -1;
            if (key.Mz > query.HighMz)
                return 1;
            return 0;
        }

        #endregion
    }

    public static class PeakArraySearch
    {

        public static ArrayIndex<PeakMzKey> CreateMzIndex(this Peak1DArray pa, bool sort)
        {
            return ArrayIndex.BuildIndex<Peak1D, PeakMzKey>(
                pa.Peaks,                
                PeakMzKey.CreateKey,
                sort);
        }

        public static IEnumerable<Peak1D> Find(
            this ArrayIndex<PeakMzKey> index,
            Peak1DArray pa,            
            double mz,
            double mzOffsetLow,
            double mzOffsetHeigh)
        {
            return index.BinarySearch(pa.Peaks, new MzRangeQuery(mz, mzOffsetLow, mzOffsetHeigh), new PeakArrayMzIndexComparer());
        }

        public static ArrayIndex<PeakMzKey> CreateMzIndex(this Peak2DArray pa, bool sort)
        {
            return ArrayIndex.BuildIndex<Peak2D, PeakMzKey>(
                pa.Peaks,
                PeakMzKey.CreateKey,
                sort);
        }

        public static IEnumerable<Peak2D> Find(
            this Peak2DArray pa,
            ArrayIndex<PeakMzKey> index,
            double mz,
            double mzOffsetLow,
            double mzOffsetHeigh)
        {
            return index.BinarySearch(pa.Peaks, new MzRangeQuery(mz, mzOffsetLow, mzOffsetHeigh), new PeakArrayMzIndexComparer());
        }
    }
}
