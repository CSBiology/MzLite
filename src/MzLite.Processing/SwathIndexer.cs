#region license
// The MIT License (MIT)

// SwathReader.cs

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
using MzLite.IO;
using MzLite.MetaData.PSIMS;
using MzLite.Model;
using System.Linq;
using MzLite.Binary;

namespace MzLite.Processing
{
    public sealed class SwathIndexer
    {

        private readonly SwathList swathList;        

        private SwathIndexer(SwathList swathList)
        {
            this.swathList = swathList;
        }

        public static SwathIndexer Create(IMzLiteDataReader dataReader, string runID)
        {

            if (dataReader == null)
                throw new ArgumentNullException("dataReader");
            if (string.IsNullOrWhiteSpace(runID))
                throw new ArgumentNullException("runID");

            var spectra = SwathSpectrumEntry.Scan(dataReader.ReadMassSpectra(runID));
            var groups = spectra.GroupBy(x => x.SwathWindow, new SwathWindowGroupingComparer()).ToArray();
            var swathes = new MSSwath[groups.Length];

            for (int i = 0; i < groups.Length; i++)
            {
                swathes[i] = new MSSwath(groups[i].Key, groups[i].ToArray());
            }

            SwathList swathList = new SwathList(swathes);

            return new SwathIndexer(swathList);
        }        

        public Peak2D[] GetMS2(
            IMzLiteDataReader dataReader,
            SwathQuery query,
            Func<IEnumerable<Peak1D>, RangeQuery, Peak1D> mzRangeSelector = null)
        {

            if (dataReader == null)
                throw new ArgumentNullException("dataReader");
            if (query == null)
                throw new ArgumentNullException("query");
            if (mzRangeSelector == null)
                mzRangeSelector = GetClosestMz;

            MSSwath swath = swathList.SearchClosestTargetMz(query);

            if (swath == null)
                return new Peak2D[0];

            SwathSpectrumEntry swathSpec = swath.SearchClosestRt(query);

            if (swathSpec == null)
                return new Peak2D[0];

            Peak1DArray spectrumPeaks = dataReader.ReadSpectrumPeaks(swathSpec.SpectrumID);

            Peak2D[] ms2Peaks = new Peak2D[query.CountMS2Masses];

            for (int i = 0; i < query.CountMS2Masses; i++)
            {
                RangeQuery mzRange = query[i];                
                IEnumerable<Peak1D> mzPeaks = BinarySearch.Search(spectrumPeaks.Peaks, mzRange, MzRangeCompare);
                Peak1D p = mzRangeSelector(mzPeaks, mzRange);
                ms2Peaks[i] = new Peak2D(p.Intensity, p.Mz, swathSpec.Rt);
            }

            return ms2Peaks;
        }
                
        //public Peak2D[,] GetRtProfiles(
        //    IMzLiteDataReader dataReader,
        //    SwathQuery query,
        //    bool getLockMz,
        //    Func<Peak1DArray, RangeQuery, Peak1D> mzRangeSelector)
        //{
        //    var swathSpectra = swathList.SearchAllTargetMz(query)
        //        .SelectMany(x => x.SearchAllRt(query))
        //        .ToArray();

        //    if (swathSpectra.Length > 0)
        //    {
        //        Peak2D[,] profile = new Peak2D[query.CountMS2Masses, swathSpectra.Length];

        //        for (int specIdx = 0; specIdx < swathSpectra.Length; specIdx++)
        //        {
        //            var swathSpec = swathSpectra[specIdx];
        //            var pa = dataReader.ReadSpectrumPeaks(swathSpec.SpectrumID);

        //            for (int ms2MassIndex = 0; ms2MassIndex < query.CountMS2Masses; ms2MassIndex++)
        //            {
        //                RangeQuery mzRange = query[ms2MassIndex];
        //                Peak1D p = mzRangeSelector(pa, mzRange);

        //                if (getLockMz)
        //                {
        //                    profile[ms2MassIndex, specIdx] = new Peak2D(p.Intensity, mzRange.LockValue, swathSpec.Rt);
        //                }
        //                else
        //                {
        //                    profile[ms2MassIndex, specIdx] = new Peak2D(p.Intensity, p.Mz, swathSpec.Rt);
        //                }
        //            }
        //        }


        //        return profile;
        //    }
        //    else
        //    {
        //        return empty2D;
        //    }
        //}
        
        /// <summary>
        /// The default mz range peak selector function.
        /// </summary>  
        private static Peak1D GetClosestMz(IEnumerable<Peak1D> peaks, RangeQuery mzRange)
        {
            return peaks
                    .DefaultIfEmpty(new Peak1D(0, mzRange.LockValue))
                    .ItemAtMin(x => Math.Abs(x.Mz - mzRange.LockValue));
        }

        private static int MzRangeCompare(Peak1D p, RangeQuery mzRange)
        {
            if (p.Mz < mzRange.LowValue)
                return -1;
            if (p.Mz > mzRange.HighValue)
                return 1;
            return 0;
        }

        internal class SwathList
        {

            private readonly MSSwath[] swathes;

            internal SwathList(MSSwath[] swathes)
            {
                this.swathes = swathes;
                Array.Sort(this.swathes, new MSSwathSortingComparer());
            }
            
            internal IEnumerable<MSSwath> SearchAllTargetMz(double targetMz)
            {
                return BinarySearch.Search(swathes, targetMz, SearchCompare);
            }

            internal MSSwath SearchClosestTargetMz(SwathQuery query)
            {
                IndexRange result;

                if (BinarySearch.Search(swathes, query.TargetMz, SearchCompare, out result))
                {
                    return IndexRange.EnumRange(swathes, result)
                        .ItemAtMin(x => CalcTargetMzDiffAbs(x, query));
                }
                else
                {
                    return null;
                }
            }

            private static double CalcTargetMzDiffAbs(MSSwath swath, SwathQuery query)
            {
                return Math.Abs(swath.SwathWindow.TargetMz - query.TargetMz);
            }

            private static int SearchCompare(MSSwath item, double targetMz)
            {
                if (item.SwathWindow.HeighMz < targetMz)
                    return -1;

                if (item.SwathWindow.LowMz > targetMz)
                    return 1;

                return 0;
            }
        }

        internal class MSSwath
        {

            private readonly SwathSpectrumEntry[] swathSpectra;

            internal MSSwath(SwathWindow sw, SwathSpectrumEntry[] swathSpectra)
            {
                this.SwathWindow = sw;
                this.swathSpectra = swathSpectra;

                Array.Sort(this.swathSpectra, new SwathSpectrumSortingComparer());
            }

            internal SwathWindow SwathWindow { get; private set; }

            internal IEnumerable<SwathSpectrumEntry> SwathSpectra { get { return swathSpectra; } }

            internal IEnumerable<SwathSpectrumEntry> SearchAllRt(SwathQuery query)
            {
                return BinarySearch.Search(swathSpectra, query, RtRangeCompare);
            }

            internal SwathSpectrumEntry SearchClosestRt(SwathQuery query)
            {
                IndexRange result;

                if (BinarySearch.Search(swathSpectra, query, RtRangeCompare, out result))
                {
                    return IndexRange.EnumRange(swathSpectra, result)
                        .ItemAtMin(x => CalcLockRtDiffAbs(x, query));
                }
                else
                {
                    return null;
                }
            }

            private static double CalcLockRtDiffAbs(SwathSpectrumEntry swathSpectrum, SwathQuery query)
            {
                return Math.Abs(swathSpectrum.Rt - query.RtRange.LockValue);
            }

            private static int RtRangeCompare(SwathSpectrumEntry item, SwathQuery query)
            {
                if (item.Rt < query.RtRange.LowValue)
                    return -1;

                if (item.Rt > query.RtRange.HighValue)
                    return 1;

                return 0;
            }

        }

        internal class SwathWindow
        {

            internal SwathWindow(double targetMz, double lowMz, double heighMz)
            {
                this.TargetMz = targetMz;
                this.LowMz = lowMz;
                this.HeighMz = heighMz;
            }

            internal double TargetMz { get; private set; }
            internal double LowMz { get; private set; }
            internal double HeighMz { get; private set; }

        }

        internal class SwathSpectrumEntry
        {

            internal SwathSpectrumEntry(string spectrumID, double targetMz, double lowMz, double heighMz, double rt)
            {
                this.SpectrumID = spectrumID;
                this.SwathWindow = new SwathWindow(targetMz, lowMz, heighMz);
                this.Rt = rt;
            }

            internal string SpectrumID { get; private set; }
            internal SwathWindow SwathWindow { get; private set; }
            internal double Rt { get; private set; }

            internal static IEnumerable<SwathSpectrumEntry> Scan(IEnumerable<MassSpectrum> spectra)
            {

                SwathSpectrumEntry sws;

                foreach (var ms in spectra)
                {
                    if (TryCreateSwathSpectrum(ms, out sws))
                        yield return sws;
                }
            }

            private static bool TryCreateSwathSpectrum(MassSpectrum ms, out SwathSpectrumEntry sws)
            {

                sws = null;
                int msLevel;

                if (!ms.TryGetMsLevel(out msLevel) || msLevel != 2)
                    return false;

                if (ms.Precursors.Count < 1 ||
                    ms.Precursors[0].SelectedIons.Count < 1 ||
                    ms.Scans.Count < 1)
                    return false;

                double rt, mz, mzLow, mzHeigh;
                var isoWin = ms.Precursors[0].IsolationWindow;
                var scan = ms.Scans[0];

                if (scan.TryGetScanStartTime(out rt)
                    && isoWin.TryGetIsolationWindowTargetMz(out mz)
                    && isoWin.TryGetIsolationWindowLowerOffset(out mzLow)
                    && isoWin.TryGetIsolationWindowUpperOffset(out mzHeigh))
                {

                    sws = new SwathSpectrumEntry(
                        ms.ID,
                        mz,
                        mz - mzLow,
                        mz + mzHeigh,
                        rt);

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        internal class SwathWindowGroupingComparer : IEqualityComparer<SwathWindow>
        {

            #region IEqualityComparer<SwathWindow> Members

            bool IEqualityComparer<SwathWindow>.Equals(SwathWindow x, SwathWindow y)
            {
                return x.LowMz.Equals(y.LowMz) && x.TargetMz.Equals(y.TargetMz) && x.HeighMz.Equals(y.HeighMz);
            }

            int IEqualityComparer<SwathWindow>.GetHashCode(SwathWindow obj)
            {
                return Tuple.Create(obj.LowMz, obj.TargetMz, obj.HeighMz).GetHashCode();
            }

            #endregion
        }

        internal class MSSwathSortingComparer : IComparer<MSSwath>
        {

            #region IComparer<MSSwath> Members

            int IComparer<MSSwath>.Compare(MSSwath x, MSSwath y)
            {
                int c = x.SwathWindow.LowMz.CompareTo(y.SwathWindow.LowMz);

                if (c != 0)
                    return c;

                return x.SwathWindow.HeighMz.CompareTo(y.SwathWindow.HeighMz);
            }

            #endregion
        }

        internal class SwathSpectrumSortingComparer : IComparer<SwathSpectrumEntry>
        {

            #region IComparer<SwathSpectrum> Members

            int IComparer<SwathSpectrumEntry>.Compare(SwathSpectrumEntry x, SwathSpectrumEntry y)
            {
                return x.Rt.CompareTo(y.Rt);
            }

            #endregion
        }
    }

    public sealed class SwathQuery
    {

        private readonly double targetMz;
        private readonly RangeQuery rtRange;
        private readonly RangeQuery[] ms2Masses;

        public SwathQuery(double targetMz, RangeQuery rtRange, params RangeQuery[] ms2Masses)
        {

            if (ms2Masses == null)
                throw new ArgumentNullException("ms2Masses");
            
            this.targetMz = targetMz;
            this.rtRange = rtRange;
            this.ms2Masses = ms2Masses;
        }

        public double TargetMz { get { return targetMz; } }
        public RangeQuery RtRange { get { return rtRange; } }
        public RangeQuery this[int idx] { get { return ms2Masses[idx]; } }
        public int CountMS2Masses { get { return ms2Masses.Length; } }
    }

    public sealed class SwathQuerySorting : IComparer<SwathQuery>
    {

        private SwathQuerySorting() { }

        public static void Sort(SwathQuery[] queries)
        {
            Array.Sort(queries, new SwathQuerySorting());
        }

        #region IComparer<SwathQuery> Members

        public int Compare(SwathQuery x, SwathQuery y)
        {
            int res = x.TargetMz.CompareTo(y.TargetMz);

            if (res != 0)
                return res;

            return x.RtRange.LockValue.CompareTo(y.RtRange.LockValue);
        }

        #endregion
    }
}
