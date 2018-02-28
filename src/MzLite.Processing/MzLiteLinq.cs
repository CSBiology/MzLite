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
using MzLite.Binary;
using MzLite.Commons.Arrays;
using MzLite.Model;
using MzLite.MetaData.PSIMS;
using MzLite.IO;
using System.Linq;

namespace MzLite.Processing
{
    public static class MzLiteLinq
    {

        /// <summary>
        /// Find the first item at val function mininum value.
        /// </summary>       
        public static T ItemAtMin<T, TValue>(
            this IEnumerable<T> source,
            Func<T, TValue> valFunc)
            where TValue : IComparable<TValue>
        {

            if (source == null)
                throw new ArgumentNullException("source");
            if (valFunc == null)
                throw new ArgumentNullException("valFunc");

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

        /// <summary>
        /// Find the first item at val function maximum value.
        /// </summary> 
        public static T ItemAtMax<T, TValue>(
            this IEnumerable<T> source,
            Func<T, TValue> valFunc)
            where TValue : IComparable<TValue>
        {

            if (source == null)
                throw new ArgumentNullException("source");
            if (valFunc == null)
                throw new ArgumentNullException("valFunc");

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

        /// <summary>
        /// Extract a rt profile matrix for specified target masses and rt range.
        /// Mz range peak aggregation is closest lock mz.
        /// </summary>        
        /// <returns>
        /// Profile matrix with first index corresponds to continous mass spectra over rt range 
        /// and second index corresponds to mz ranges given.
        /// </returns>
        public static Peak2D[,] RtProfiles(
            this IMzLiteDataReader dataReader,
            IMzLiteArray<RtIndexEntry> rtIndex,
            RangeQuery rtRange,
            params RangeQuery[] mzRanges)
        {

            if (dataReader == null)
                throw new ArgumentNullException("dataReader");
            if (rtIndex == null)
                throw new ArgumentNullException("rtIndex");
            if (mzRanges == null)
                throw new ArgumentNullException("mzRanges");

            var entries = rtIndex.Search(rtRange).ToArray();
            var profile = new Peak2D[entries.Length, mzRanges.Length];

            for (int rtIdx = 0; rtIdx < entries.Length; rtIdx++)
            {
                var entry = entries[rtIdx];
                var peaks = dataReader.ReadSpectrumPeaks(entry).Peaks;

                for (int mzIdx = 0; mzIdx < mzRanges.Length; mzIdx++)
                {
                    var mzRange = mzRanges[mzIdx];

                    var p = peaks.MzSearch(mzRange)
                        .DefaultIfEmpty(new Peak1D(0, mzRange.LockValue))
                        .ClosestMz(mzRange.LockValue)
                        .AsPeak2D(entry.Rt);

                    profile[rtIdx, mzIdx] = p;
                }
            }

            return profile;
        }
        /// <summary>
        /// Extract a rt profile for specified target mass and rt range.
        /// Mz range peak aggregation is closest lock mz.
        /// </summary>        
        /// <returns>
        /// Profile array with index corresponding to continous mass spectra over rt range and mz range given.
        /// </returns>
        public static Peak2D[] RtProfile(
            this IMzLiteDataReader dataReader,
            IMzLiteArray<RtIndexEntry> rtIndex,
            RangeQuery rtRange,
            RangeQuery mzRange)
        {

            if (dataReader == null)
                throw new ArgumentNullException("dataReader");
            if (rtIndex == null)
                throw new ArgumentNullException("rtIndex");

            var entries = rtIndex.Search(rtRange).ToArray();
            var profile = new Peak2D[entries.Length];

            for (int rtIdx = 0; rtIdx < entries.Length; rtIdx++)
            {
                var entry = entries[rtIdx];
                var peaks = dataReader.ReadSpectrumPeaks(entry).Peaks;
                var p = peaks.MzSearch(mzRange)
                        .DefaultIfEmpty(new Peak1D(0, mzRange.LockValue))
                        .ClosestMz(mzRange.LockValue)
                        .AsPeak2D(entry.Rt);
                profile[rtIdx] = p;
            }
            return profile;
        }
        /// <summary>
        /// Builds an in memory retention time index of mass spectra ids.
        /// </summary>        
        public static IMzLiteArray<RtIndexEntry> BuildRtIndex(
            this IMzLiteDataReader dataReader,
            string runID,
            int msLevel = 1)
        {
            if (dataReader == null)
                throw new ArgumentNullException("dataReader");
            if (string.IsNullOrWhiteSpace(runID))
                throw new ArgumentNullException("runID");

            IEnumerable<MassSpectrum> massSpectra = dataReader.ReadMassSpectra(runID);
            List<RtIndexEntry> entries = new List<RtIndexEntry>();
            RtIndexEntry entry;

            foreach (var ms in massSpectra)
            {
                if (TryCreateEntry(ms, msLevel, out entry))
                    entries.Add(entry);
            }

            entries.Sort(new RtIndexEntrySorting());

            return MzLiteArray.ToMzLiteArray(entries);
        }

        /// <summary>
        /// Get all rt index entries by rt range.
        /// </summary> 
        public static IEnumerable<RtIndexEntry> Search(
            this IMzLiteArray<RtIndexEntry> rti,
            RangeQuery rtRange)
        {
            if (rti == null)
                throw new ArgumentNullException("rti");

            return BinarySearch.Search(rti, rtRange, RtSearchCompare);
        }

        public static Peak1DArray ReadSpectrumPeaks(
            this IMzLiteDataReader dataReader,
            RtIndexEntry entry)
        {
            if (dataReader == null)
                throw new ArgumentNullException("dataReader");

            return dataReader.ReadSpectrumPeaks(entry.SpectrumID);
        }

        public static MassSpectrum ReadMassSpectrum(
            this IMzLiteDataReader dataReader,
            RtIndexEntry entry)
        {
            if (dataReader == null)
                throw new ArgumentNullException("dataReader");

            return dataReader.ReadMassSpectrum(entry.SpectrumID);
        }

        /// <summary>
        /// Get all peaks by mz range.
        /// </summary>        
        public static IEnumerable<TPeak> MzSearch<TPeak>(
            this IMzLiteArray<TPeak> peaks,
            RangeQuery mzRange) where TPeak : Peak1D
        {
            if (peaks == null)
                throw new ArgumentNullException("peaks");

            return BinarySearch.Search(peaks, mzRange, MzSearchCompare);
        }

        /// <summary>
        /// Get all peaks by rt range.
        /// </summary> 
        public static IEnumerable<TPeak> RtSearch<TPeak>(
            this IMzLiteArray<TPeak> peaks,
            RangeQuery rtRange) where TPeak : Peak2D
        {
            if (peaks == null)
                throw new ArgumentNullException("peaks");

            return BinarySearch.Search(peaks, rtRange, RtSearchCompare);
        }

        /// <summary>
        /// Gets the peak closest to lock mz.
        /// </summary>        
        public static TPeak ClosestMz<TPeak>(
            this IEnumerable<TPeak> peaks,
            double lockMz)
            where TPeak : Peak1D
        {
            if (peaks == null)
                throw new ArgumentNullException("peaks");

            return peaks.ItemAtMin(x => Math.Abs(x.Mz - lockMz));
        }

        /// <summary>
        /// Gets the peak closest to lock rt.
        /// </summary>        
        public static TPeak ClosestRt<TPeak>(
            this IEnumerable<TPeak> peaks,
            double lockRt)
            where TPeak : Peak2D
        {
            if (peaks == null)
                throw new ArgumentNullException("peaks");

            return peaks.ItemAtMin(x => Math.Abs(x.Rt - lockRt));
        }

        /// <summary>
        /// Gets the peak at max intensity.
        /// </summary>        
        public static TPeak MaxIntensity<TPeak>(
            this IEnumerable<TPeak> peaks)
            where TPeak : Peak
        {
            if (peaks == null)
                throw new ArgumentNullException("peaks");

            return peaks.ItemAtMax(x => x.Intensity);
        }

        public static Peak2D AsPeak2D(
            this Peak1D p,
            double rt)
        {
            if (p == null)
                throw new ArgumentNullException("p");
            return new Peak2D(p.Intensity, p.Mz, rt);
        }

        private static bool TryCreateEntry(
            MassSpectrum ms,
            int msLevel,
            out RtIndexEntry entry)
        {
            entry = default(RtIndexEntry);
            int _msLevel;

            if (!ms.TryGetMsLevel(out _msLevel) || _msLevel != msLevel)
                return false;

            if (ms.Scans.Count < 1)
                return false;

            double rt;
            Scan scan = ms.Scans[0];

            if (scan.TryGetScanStartTime(out rt))
            {
                entry = new RtIndexEntry(rt, ms.ID);
                return true;
            }
            else
            {
                return false;
            }
        }

        private static int RtSearchCompare(
            RtIndexEntry entry,
            RangeQuery rtRange)
        {
            if (entry.Rt < rtRange.LowValue)
                return -1;

            if (entry.Rt > rtRange.HighValue)
                return 1;

            return 0;
        }

        private static int MzSearchCompare<TPeak>(
            TPeak p,
            RangeQuery mzRange)
            where TPeak : Peak1D
        {
            if (p.Mz < mzRange.LowValue)
                return -1;
            if (p.Mz > mzRange.HighValue)
                return 1;
            return 0;
        }

        private static int RtSearchCompare<TPeak>(
            TPeak p,
            RangeQuery rtRange)
            where TPeak : Peak2D
        {
            if (p.Rt < rtRange.LowValue)
                return -1;
            if (p.Rt > rtRange.HighValue)
                return 1;
            return 0;
        }

        private class RtIndexEntrySorting : IComparer<RtIndexEntry>
        {
            #region IComparer<RtIndexEntry> Members

            int IComparer<RtIndexEntry>.Compare(RtIndexEntry x, RtIndexEntry y)
            {
                return x.Rt.CompareTo(y.Rt);
            }

            #endregion
        }

        public struct RtIndexEntry
        {

            private readonly double rt;
            private readonly string spectrumID;

            internal RtIndexEntry(double rt, string spectrumID)
            {

                if (string.IsNullOrWhiteSpace(spectrumID))
                    throw new ArgumentNullException("spectrumID");

                this.rt = rt;
                this.spectrumID = spectrumID;
            }

            public double Rt { get { return rt; } }
            public string SpectrumID { get { return spectrumID; } }

            public override string ToString()
            {
                return string.Format("[rt={0}, spectrumID='{1}']", rt, spectrumID);
            }
        }
    }
}
