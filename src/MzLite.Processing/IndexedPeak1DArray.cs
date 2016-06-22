using System;
using System.Collections.Generic;
using System.Linq;
using MzLite.Binary;

namespace MzLite.Processing
{
    public sealed class IndexedPeak1DArray
    {

        private readonly string spectrumID;
        private readonly Peak1DArray peakArray;
        private readonly MzKey[] keys;

        public IndexedPeak1DArray(string spectrumID, Peak1DArray peakArray, bool assumePeaksMzSorted)
        {
            this.spectrumID = spectrumID;
            this.peakArray = peakArray;
            this.keys = new MzKey[peakArray.Peaks.Length];

            for (int i = 0; i < peakArray.Peaks.Length; i++)
                keys[i] = new MzKey(peakArray.Peaks[i].Mz, i);

            if (!assumePeaksMzSorted)
                Array.Sort(keys);
        }

        public IEnumerable<Peak1D> SearchAll(MzRangeQuery query)
        {
            IndexRange result;

            if (BinarySearch.Search(keys, query, SearchCompare, out result))
            {
                return EnumRange(result);
            }
            else
            {
                return Enumerable.Empty<Peak1D>();
            }
        }

        public Peak1D SearchClosestMz(MzRangeQuery query)
        {
            IndexRange result;

            if (BinarySearch.Search(keys, query, SearchCompare, out result))
            {
                return EnumRange(result).ItemAtMin(x => CalcLockMzDiffAbs(x, query));
            }
            else
            {
                return null;
            }
        }

        public Peak1D SearchMaxIntensity(MzRangeQuery query)
        {
            IndexRange result;

            if (BinarySearch.Search(keys, query, SearchCompare, out result))
            {
                return EnumRange(result).ItemAtMax(x => x.Intensity);
            }
            else
            {
                return null;
            }
        }

        private IEnumerable<Peak1D> EnumRange(IndexRange range)
        {
            return IndexRange.EnumRange(peakArray.Peaks, keys, x=>x.ArrayIndex, range);
        }

        private static double CalcLockMzDiffAbs(Peak1D p, MzRangeQuery query)
        {
            return Math.Abs(p.Mz - query.LockMz);
        }

        private static int SearchCompare(MzKey key, MzRangeQuery query)
        {
            if (key.Mz < query.LowMz)
                return -1;
            if (key.Mz > query.HighMz)
                return 1;
            return 0;
        }                

        internal class MzKey : IComparable<MzKey>
        {

            internal MzKey(double mz, int index)
            {
                this.Mz = mz;
                this.ArrayIndex = index;
            }

            internal double Mz { get; private set; }
            internal int ArrayIndex { get; private set; }

            #region IComparable<MzKey> Members

            int IComparable<MzKey>.CompareTo(MzKey other)
            {
                return Mz.CompareTo(other.Mz);
            }

            #endregion
        }
        
    }

    public sealed class MzRangeQuery
    {
        private readonly double lockMz;
        private readonly double lowMz;
        private readonly double highMz;

        public MzRangeQuery(
            double lockMz,
            double mzOffsetLow,
            double mzOffsetHeigh)
        {
            this.lockMz = lockMz;
            this.lowMz = lockMz - mzOffsetLow;
            this.highMz = lockMz + mzOffsetHeigh;
        }

        public double LockMz { get { return lockMz; } }
        public double LowMz { get { return lowMz; } }
        public double HighMz { get { return highMz; } }
    }
}
