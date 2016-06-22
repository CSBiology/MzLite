using System;
using System.Collections.Generic;
using MzLite.IO;
using MzLite.MetaData;
using MzLite.Model;
using System.Linq;
using MzLite.Binary;

namespace MzLite.Processing
{
    public sealed class SwathReader : IDisposable
    {

        private readonly IMzLiteDataReader dataReader;
        private readonly string runID;
        private bool isDisposed;        
        private readonly SwathList swathList;
        private readonly bool assumePeaksMzSorted;
        private static readonly Peak1D[] empty1D = new Peak1D[0];
        private static readonly Peak2D[] empty2D = new Peak2D[0];

        private SwathReader(IMzLiteDataReader dataReader, string runID, SwathList swathList, bool assumePeaksMzSorted)
        {
            this.dataReader = dataReader;
            this.runID = runID;
            this.swathList = swathList;
            this.assumePeaksMzSorted = assumePeaksMzSorted;
        }

        public static SwathReader Create(IMzLiteDataReader dataReader, string runID, bool assumePeaksMzSorted)
        {
            var spectra = SwathSpectrum.Scan(dataReader.ReadMassSpectra(runID));
            var groups = spectra.GroupBy(x => x.SwathWindow, new SwathWindowGroupingComparer()).ToArray();
            var swathes = new MSSwath[groups.Length];

            for (int i = 0; i < groups.Length; i++)
            {
                swathes[i] = new MSSwath(groups[i].Key, groups[i].ToArray());
            }

            SwathList swathList = new SwathList(swathes);

            return new SwathReader(dataReader, runID, swathList, assumePeaksMzSorted);                 
        }

        public Peak1D[] GetMS2(
            SwathQuery query,
            bool getLockMz)
        {
            var swath = swathList.SearchClosestTargetMz(query);
            
            if (swath == null)
                return empty1D;
                     
            var swathSpec = swath.SearchClosestRt(query);

            if (swathSpec == null)
                return empty1D;

            var pa = dataReader.ReadSpectrumPeaks(swathSpec.SpectrumID);
            var ms2Index = new IndexedPeak1DArray(swathSpec.SpectrumID, pa, assumePeaksMzSorted);

            var peaks = new Peak1D[query.MS2Masses.Length];

            for (int i = 0; i < query.MS2Masses.Length; i++)
            {
                var ms2Query = query.MS2Masses[i];
                var closestMz = ms2Index.SearchAll(ms2Query)
                    .DefaultIfEmpty(new Peak1D(0, ms2Query.LockMz))
                    .ItemAtMin(x => Math.Abs(x.Mz - ms2Query.LockMz));

                if (getLockMz)
                {
                    peaks[i] = new Peak1D(closestMz.Intensity, ms2Query.LockMz);
                }
                else
                {
                    peaks[i] = closestMz;
                }
            }

            return peaks;
        }

        public Peak2D[] GetRtProfile(
            SwathQuery query,
            int ms2MassIndex,
            bool getLockMz)
        {
            var swathSpectra = swathList.SearchAll(query)
                .SelectMany(x => x.SearchAll(query))
                .ToArray();

            var ms2Query = query.MS2Masses[ms2MassIndex];

            if (swathSpectra.Length > 0)
            {
                Peak2D[] profile = new Peak2D[swathSpectra.Length];

                for (int i = 0; i < swathSpectra.Length; i++)
                {
                    var swathSpec = swathSpectra[i];
                    var pa = dataReader.ReadSpectrumPeaks(swathSpec.SpectrumID);
                    var ms2Index = new IndexedPeak1DArray(swathSpec.SpectrumID, pa, assumePeaksMzSorted);
                    var closestMz = ms2Index.SearchAll(ms2Query)
                        .DefaultIfEmpty(new Peak1D(0, ms2Query.LockMz))
                        .ItemAtMin(x => Math.Abs(x.Mz - ms2Query.LockMz));

                    if (getLockMz)
                        profile[i] = new Peak2D(closestMz.Intensity, ms2Query.LockMz, swathSpec.Rt);
                    else
                        profile[i] = new Peak2D(closestMz.Intensity, closestMz.Mz, swathSpec.Rt);
                }

                return profile;

            }
            else
            {
                return empty2D;
            }
        }

        
        #region IDisposable Members

        private void RaiseDisposed()
        {
            if (isDisposed)
                throw new ObjectDisposedException(this.GetType().Name);
        }

        public void Dispose()
        {
            if (isDisposed)
                return;            
            isDisposed = true;
        }

        #endregion

        internal class SwathList
        {

            private readonly MSSwath[] swathes;

            internal SwathList(MSSwath[] swathes)
            {
                this.swathes = swathes;
                Array.Sort(this.swathes, new MSSwathSortingComparer());
            }

            internal IEnumerable<MSSwath> SearchAll(SwathQuery query)
            {
                return SearchAll(query.TargetMz);                
            }

            internal IEnumerable<MSSwath> SearchAll(double targetMz)
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

            private readonly SwathSpectrum[] swathSpectra;

            internal MSSwath(SwathWindow sw, SwathSpectrum[] swathSpectra)
            {
                this.SwathWindow = sw;
                this.swathSpectra = swathSpectra;

                Array.Sort(this.swathSpectra, new SwathSpectrumSortingComparer());
            }

            internal SwathWindow SwathWindow { get; private set; }

            internal IEnumerable<SwathSpectrum> SwathSpectra { get { return swathSpectra; } }

            internal IEnumerable<SwathSpectrum> SearchAll(SwathQuery query)
            {
                return BinarySearch.Search(swathSpectra, query, SearchCompare);
            }

            internal SwathSpectrum SearchClosestRt(SwathQuery query)
            {
                IndexRange result;

                if (BinarySearch.Search(swathSpectra, query, SearchCompare, out result))
                {
                    return IndexRange.EnumRange(swathSpectra, result)
                        .ItemAtMin(x => CalcLockRtDiffAbs(x, query));
                }
                else
                {
                    return null;
                }
            }
            
            private static double CalcLockRtDiffAbs(SwathSpectrum swathSpectrum, SwathQuery query)
            {
                return Math.Abs(swathSpectrum.Rt - query.LockRt);
            }

            private static int SearchCompare(SwathSpectrum item, SwathQuery query)
            {
                if (item.Rt < query.LowRt)
                    return -1;

                if (item.Rt > query.HeighRt)
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

        internal class SwathSpectrum
        {

            internal SwathSpectrum(string spectrumID, double targetMz, double lowMz, double heighMz, double rt)
            {
                this.SpectrumID = spectrumID;
                this.SwathWindow = new SwathWindow(targetMz, lowMz, heighMz);                
                this.Rt = rt;
            }

            internal string SpectrumID { get; private set; }
            internal SwathWindow SwathWindow { get; private set; }
            internal double Rt { get; private set; }

            internal static IEnumerable<SwathSpectrum> Scan(IEnumerable<MassSpectrum> spectra)
            {

                SwathSpectrum sws;

                foreach (var ms in spectra)
                {
                    if (TryCreateSwathSpectrum(ms, out sws))
                        yield return sws;
                }
            }

            private static bool TryCreateSwathSpectrum(MassSpectrum ms, out SwathSpectrum sws)
            {

                sws = null;

                if (ms.BeginParamEdit().Get_MS_Level().GetInt32() != 2)
                    return false;

                if (ms.Precursors.Count < 1 ||
                    ms.Precursors[0].SelectedIons.Count < 1 ||
                    ms.Scans.Count < 1)
                    return false;

                IValueConverter rt = ms.Scans[0].BeginParamEdit().Get_MS_ScanStartTime();
                IParamEdit isoWin = ms.Precursors[0].IsolationWindow.BeginParamEdit();
                IValueConverter mz = isoWin.Get_MS_IsolationWindowTargetMz();
                IValueConverter mzLow = isoWin.Get_MS_IsolationWindowLowerOffset();
                IValueConverter mzHigh = isoWin.Get_MS_IsolationWindowUpperOffset();

                if (rt.HasValue() &&
                    mz.HasValue() &&
                    mzLow.HasValue() &&
                    mzHigh.HasValue())
                {

                    double mzValue = mz.GetDouble();

                    sws = new SwathSpectrum(
                        ms.ID,
                        mzValue,
                        mzValue - mzLow.GetDouble(),
                        mzValue + mzHigh.GetDouble(),
                        rt.GetDouble());

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

            #region IEqualityComparer<MzWindow> Members

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

            #region IComparer<SwathMSExperiment> Members

            int IComparer<MSSwath>.Compare(MSSwath x, MSSwath y)
            {
                int c = x.SwathWindow.LowMz.CompareTo(y.SwathWindow.LowMz);

                if (c != 0)
                    return c;
                
                return x.SwathWindow.HeighMz.CompareTo(y.SwathWindow.HeighMz);
            }

            #endregion
        }

        internal class SwathSpectrumSortingComparer : IComparer<SwathSpectrum>
        {

            #region IComparer<TargetSpectrum> Members

            int IComparer<SwathSpectrum>.Compare(SwathSpectrum x, SwathSpectrum y)
            {
                return x.Rt.CompareTo(y.Rt);
            }

            #endregion
        }
    }

    public sealed class SwathQuery
    {

        private readonly double targetMz;
        private readonly double lockRt;
        private readonly double lowRt;
        private readonly double heighRt;
        private readonly MzRangeQuery[] ms2Masses;

        public SwathQuery(double targetMz, double lockRt, double rtOffsetLow, double rtOffsetHeigh, params MzRangeQuery[] ms2Masses)
        {
            this.targetMz = targetMz;
            this.lockRt = lockRt;
            this.lowRt = lockRt - rtOffsetLow;
            this.heighRt = lockRt + rtOffsetHeigh;
            this.ms2Masses = ms2Masses;
        }

        public double TargetMz { get { return targetMz; } }
        public double LockRt { get { return lockRt; } }
        public double LowRt { get { return lowRt; } }
        public double HeighRt { get { return heighRt; } }
        public MzRangeQuery[] MS2Masses { get { return ms2Masses; } }
    }
}
