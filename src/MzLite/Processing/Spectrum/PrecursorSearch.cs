using System;
using System.Collections.Generic;
using MzLite.MetaData;
using MzLite.Model;
using MzLite.Processing.Indexing;

namespace MzLite.Processing.Spectrum
{

    public sealed class TargetMzQuery
    {
        private readonly double mz;
        private readonly double lowRt;
        private readonly double heighRt;

        public TargetMzQuery(double mz, double lowRt, double heighRt)
        {
            this.mz = mz;
            this.lowRt = lowRt;
            this.heighRt = heighRt;
        }

        public double Mz { get { return mz; } }
        public double LowRt { get { return lowRt; } }
        public double HeighRt { get { return heighRt; } }
    }

    public sealed class TargetMzKey : IDIndexKey<string>, IComparable<TargetMzKey>
    {

        private readonly double targetMz;
        private readonly double lowMz;
        private readonly double heighMz;
        private readonly double rt;

        internal TargetMzKey(double targetMz, double lowMz, double heighMz, double rt, string spectrumID)
            : base(spectrumID)
        {
            this.targetMz = targetMz;
            this.lowMz = lowMz;
            this.heighMz = heighMz;
            this.rt = rt;
        }

        public double TargetMz { get { return targetMz; } }
        public double LowMz { get { return lowMz; } }
        public double HeighMz { get { return heighMz; } }
        public double Rt { get { return rt; } }

        #region IComparable<TargetMzKey> Members

        public int CompareTo(TargetMzKey other)
        {
            if (lowMz.CompareTo(other.lowMz) != 0)
            {
                return lowMz.CompareTo(other.lowMz);
            }
            if (heighMz.CompareTo(other.heighMz) != 0)
            {
                return heighMz.CompareTo(other.heighMz);
            }
            if (rt.CompareTo(other.rt) != 0)
            {
                return rt.CompareTo(other.rt);
            }

            return 0;
        }

        #endregion
    }

    public sealed class TargetMzIndexComparer : IBinarySearchComparer<TargetMzKey, TargetMzQuery>
    {

        internal TargetMzIndexComparer() { }

        #region IBinarySearchComparer<TargetMzKey,TargetMzQuery> Members

        public int Compare(TargetMzKey key, TargetMzQuery query)
        {
            if (key.HeighMz < query.Mz)
                return -1;

            if (key.LowMz > query.Mz)
                return 1;

            if (key.Rt < query.LowRt)
                return -1;

            if (key.Rt > query.HeighRt)
                return 1;

            return 0;
        }

        #endregion
    }

    public static class PrecursorSearch
    {

        public static IDIndex<string, TargetMzKey> CreateTargetMzIndex(this IEnumerable<MassSpectrum> spectra)
        {
            return IDIndex.BuildIndex<MassSpectrum, string, TargetMzKey>(spectra, CreateTargetMzKeys, true);
        }

        public static IEnumerable<TargetMzKey> Find(
            this IDIndex<string, TargetMzKey> index,             
            double mz, 
            double rt, 
            double rtOffsetLow, 
            double rtOffsetHeigh)
        {
            return index.BinarySearch(
                new TargetMzQuery(mz, DecimalHelper.Subtract(rt, rtOffsetLow), DecimalHelper.Add(rt, rtOffsetHeigh)),
                new TargetMzIndexComparer());
        }

        public static IEnumerable<TargetMzKey> CreateTargetMzKeys(IEnumerable<MassSpectrum> spectra)
        {

            TargetMzKey key;

            foreach (var ms in spectra)
            {                
                if (TryCreateKey(ms, out key))
                    yield return key;
            }
        }

        public static bool TryCreateKey(MassSpectrum ms, out TargetMzKey key)
        {

            key = null;

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

                key = new TargetMzKey(
                    mzValue, 
                    DecimalHelper.Subtract(mzValue, mzLow.GetDouble()), 
                    DecimalHelper.Add(mzValue, mzHigh.GetDouble()), 
                    rt.GetDouble(), 
                    ms.ID);
                
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
