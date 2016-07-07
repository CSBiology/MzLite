#region license
// The MIT License (MIT)

// RtIndexer.cs

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
using MzLite.Model;
using MzLite.MetaData.PSIMS;
using System.Linq;
using MzLite.Binary;
using MzLite.Commons.Arrays;

namespace MzLite.Processing
{
    //public sealed class RtIndexer
    //{
        
    //    private readonly RtSpectrum[] rtSpectra;

    //    private RtIndexer(RtSpectrum[] rtSpectra)
    //    {
    //        this.rtSpectra = rtSpectra;
    //    }        

    //    public static RtIndexer Create(IMzLiteDataReader dataReader, string runID, int msLevel)
    //    {

    //        if (dataReader == null)
    //            throw new ArgumentNullException("dataReader");

    //        RtSpectrum[] rtSpectra = RtSpectrum.Scan(dataReader.ReadMassSpectra(runID), msLevel).ToArray();
    //        Array.Sort(rtSpectra, new RtSpectrumSortingComparer());
    //        return new RtIndexer(rtSpectra);
    //    }

    //    public int Size { get { return rtSpectra.Length; } }

    //    public int GetMaxIntensityIndex(IMzLiteDataReader dataReader, RtMzQuery query)
    //    {

    //        if (dataReader == null)
    //            throw new ArgumentNullException("dataReader");
    //        if (query == null)
    //            throw new ArgumentNullException("query");

    //        IndexRange result;

    //        if (BinarySearch.Search(rtSpectra, query.RtRange, SearchCompare, out result))
    //        {
    //            return result.Low + IndexRange.EnumRange(rtSpectra, result)
    //                .IndexAtMax(x => SearchClosestMz(dataReader, x.SpectrumID, query.MzRange, false).Intensity);
    //        }
    //        else
    //        {
    //            return -1;
    //        } 

    //    }

    //    public int GetClosestRtIndex(RtMzQuery query)
    //    {

    //        if (query == null)
    //            throw new ArgumentNullException("query");

    //        return SearchClosestRtIndex(query.RtRange);
    //    }

    //    public Peak2D GetClosestMz(IMzLiteDataReader dataReader, int idx, RtMzQuery query, bool getLockMz)
    //    {
    //        if (idx < 0 || idx > rtSpectra.Length)
    //            throw new IndexOutOfRangeException("idx");
    //        if (dataReader == null)
    //            throw new ArgumentNullException("dataReader");
    //        if (query == null)
    //            throw new ArgumentNullException("query");

    //        RtSpectrum rts = rtSpectra[idx];
    //        Peak1D p = SearchClosestMz(dataReader, rts.SpectrumID, query.MzRange, getLockMz);
    //        return new Peak2D(p.Intensity, p.Mz, rts.Rt);
    //    }

    //    public Peak2D[] GetRtProfile(
    //        IMzLiteDataReader dataReader,            
    //        RtMzQuery query,
    //        bool getLockMz)
    //    {

    //        if (dataReader == null)
    //            throw new ArgumentNullException("dataReader");
    //        if (query == null)
    //            throw new ArgumentNullException("query");

    //        IndexRange result;

    //        if (BinarySearch.Search(rtSpectra, query.RtRange, SearchCompare, out result))
    //        {

    //            Peak2D[] profile = new Peak2D[result.Length];
    //            int idx = 0;

    //            for (int i = result.Low; i <= result.Heigh; i++)
    //            {
    //                profile[idx] = GetClosestMz(dataReader, i, query, getLockMz);
    //                idx++;
    //            }

    //            return profile;
    //        }
    //        else
    //        {
    //            return new Peak2D[0];
    //        }            
    //    }

    //    #region peak search

    //    private static IEnumerable<Peak1D> SearchMz(IMzLiteDataReader dataReader, string spectrumID, RangeQuery query)
    //    {
    //        IndexRange result;
    //        var pa = dataReader.ReadSpectrumPeaks(spectrumID);

    //        if (BinarySearch.Search(pa.Peaks, query, SearchCompare, out result))
    //        {
    //            return IndexRange.EnumRange(pa.Peaks, result);
    //        }
    //        else
    //        {
    //            return Enumerable.Empty<Peak1D>();
    //        }
    //    }

    //    private static Peak1D SearchClosestMz(IMzLiteDataReader dataReader, string spectrumID, RangeQuery mzRange, bool getLockMz)
    //    {
    //        var closestMz = SearchMz(dataReader, spectrumID, mzRange)
    //                .DefaultIfEmpty(new Peak1D(0, mzRange.LockValue))
    //                .ItemAtMin(x => Math.Abs(x.Mz - mzRange.LockValue));

    //        if (getLockMz)
    //        {
    //            return new Peak1D(closestMz.Intensity, mzRange.LockValue);
    //        }
    //        else
    //        {
    //            return closestMz;
    //        }
    //    }

    //    private static int SearchCompare(Peak1D p, RangeQuery mzRange)
    //    {
    //        if (p.Mz < mzRange.LowValue)
    //            return -1;
    //        if (p.Mz > mzRange.HighValue)
    //            return 1;
    //        return 0;
    //    }

    //    #endregion

    //    #region rt search
        
    //    private int SearchClosestRtIndex(RangeQuery rtRange)
    //    {            
    //        IndexRange result;

    //        if (BinarySearch.Search(rtSpectra, rtRange, SearchCompare, out result))
    //        {
    //            return result.Low + IndexRange.EnumRange(rtSpectra, result)
    //                .IndexAtMin(x => Math.Abs(x.Rt - rtRange.LockValue));
    //        }
    //        else
    //        {
    //            return -1;
    //        }
    //    }
        
    //    private static int SearchCompare(RtSpectrum item, RangeQuery rtRange)
    //    {
    //        if (item.Rt < rtRange.LowValue)
    //            return -1;

    //        if (item.Rt > rtRange.HighValue)
    //            return 1;

    //        return 0;
    //    }

    //    internal class RtSpectrum
    //    {

    //        internal RtSpectrum(string spectrumID, double rt)
    //        {
    //            this.SpectrumID = spectrumID;
    //            this.Rt = rt;
    //        }

    //        internal string SpectrumID { get; private set; }
    //        internal double Rt { get; private set; }

    //        internal static IEnumerable<RtSpectrum> Scan(IEnumerable<MassSpectrum> spectra, int msLevel)
    //        {

    //            RtSpectrum key;

    //            foreach (var ms in spectra)
    //            {
    //                if (TryCreateSpectrumKey(ms, msLevel, out key))
    //                    yield return key;
    //            }
    //        }

    //        private static bool TryCreateSpectrumKey(MassSpectrum ms, int msLevel, out RtSpectrum key)
    //        {

    //            key = null;
    //            int _msLevel;

    //            if (!ms.TryGetMsLevel(out _msLevel) || _msLevel != msLevel)
    //                return false;

    //            if (ms.Scans.Count < 1)
    //                return false;

    //            double rt;
    //            Scan scan = ms.Scans[0];

    //            if (scan.TryGetScanStartTime(out rt))
    //            {
    //                key = new RtSpectrum(ms.ID, rt);
    //                return true;
    //            }
    //            else
    //            {
    //                return false;
    //            }
    //        }
    //    }

    //    internal class RtSpectrumSortingComparer : IComparer<RtSpectrum>
    //    {

    //        #region IComparer<RtSpectrum> Members

    //        int IComparer<RtSpectrum>.Compare(RtSpectrum x, RtSpectrum y)
    //        {
    //            return x.Rt.CompareTo(y.Rt);
    //        }

    //        #endregion
    //    }

    //    #endregion
    //}

    //public sealed class RtMzQuery
    //{
        
    //    private readonly RangeQuery rtRange;
    //    private readonly RangeQuery mzRange;
        
    //    public RtMzQuery(RangeQuery rtRange, RangeQuery mzRange)
    //    {

    //        if (mzRange == null)
    //            throw new ArgumentNullException("mzRange");
    //        if (rtRange == null)
    //            throw new ArgumentNullException("rtRange");

    //        this.rtRange = rtRange;
    //        this.mzRange = mzRange;
    //    }
        
    //    public RangeQuery RtRange { get { return rtRange; } }
    //    public RangeQuery MzRange { get { return mzRange; } }

    //}

    public static class RtIndexer
    {

        public static IMzLiteArray<RtIndexerItem> Create(
            IMzLiteDataReader dataReader, 
            string runID, 
            int msLevel)
        {
            if (dataReader == null)
                throw new ArgumentNullException("dataReader");

            RtIndexerItem[] rtSpectra = Scan(dataReader.ReadMassSpectra(runID), msLevel).ToArray();
            Array.Sort(rtSpectra, new RtIndexerSortingComparer());
            return rtSpectra.ToMzLiteArray();
        }

        public static IMzLiteArray<Peak2D> GetMassTrace(
            this IMzLiteArray<RtIndexerItem> index, 
            IMzLiteDataReader dataReader,
            RangeQuery mzRange,
            bool getLockMz)
        {
            return GetMassTrace(
                index, 
                new IndexRange(0, index.Length - 1), 
                dataReader, 
                mzRange, 
                getLockMz)
                .ToMzLiteArray();
        }

        public static IMzLiteArray<Peak2D> GetMassTrace(
            this IMzLiteArray<RtIndexerItem> index,
            IMzLiteDataReader dataReader,
            RtIndexerQuery query,
            bool getLockMz)
        {
            if (dataReader == null)
                throw new ArgumentNullException("dataReader");
            if (query == null)
                throw new ArgumentNullException("query");

            IndexRange result;

            if (BinarySearch.Search(index, query.RtRange, SearchCompare, out result))
            {
                return GetMassTrace(
                    index,
                    result,
                    dataReader,
                    query.MzRange,
                    getLockMz)
                    .ToMzLiteArray();
            }
            else
            {
                return MzLiteArray.Empty<Peak2D>();
            }
        
        }

        public static int GetMaxIntensityIndex(
            this IMzLiteArray<RtIndexerItem> index, 
            IMzLiteDataReader dataReader,
            RtIndexerQuery query)
        {

            if (dataReader == null)
                throw new ArgumentNullException("dataReader");
            if (query == null)
                throw new ArgumentNullException("query");

            IndexRange result;

            if (BinarySearch.Search(index, query.RtRange, SearchCompare, out result))
            {
                return IndexRange.EnumRange(index, result)
                    .Select((x, i) => new ItemIndex<double>(GetClosestMz(dataReader, x, query.MzRange, false).Intensity, result.GetSourceIndex(i)))                    
                    .ItemAtMax(x => x.Item)
                    .Index;
            }
            else
            {
                return -1;
            }

        }

        public static Peak2D GetClosestMz(
            this IMzLiteArray<RtIndexerItem> index, 
            IMzLiteDataReader dataReader, 
            int idx, 
            RangeQuery mzRange, 
            bool getLockMz)
        {
            if (index == null)
                throw new ArgumentNullException("index");
            if (idx < 0 || idx > index.Length)
                throw new IndexOutOfRangeException("idx");            

            RtIndexerItem item = index[idx];
            return GetClosestMz(dataReader, item, mzRange,getLockMz);            
        }

        public static Peak2D GetClosestMz(
            IMzLiteDataReader dataReader, 
            RtIndexerItem item,
            RangeQuery mzRange, 
            bool getLockMz)
        {

            if (dataReader == null)
                throw new ArgumentNullException("dataReader");
            if (item == null)
                throw new ArgumentNullException("item");
            if (mzRange == null)
                throw new ArgumentNullException("mzRange");

            IndexRange result;
            var pa = dataReader.ReadSpectrumPeaks(item.SpectrumID);

            if (BinarySearch.Search(pa.Peaks, mzRange, SearchCompare, out result))
            {
                Peak1D p = IndexRange.EnumRange(pa.Peaks, result)
                    .ItemAtMin(x => Math.Abs(x.Mz - mzRange.LockValue));

                if (getLockMz)
                    return new Peak2D(p.Intensity, mzRange.LockValue, item.Rt);
                else
                    return new Peak2D(p.Intensity, p.Mz, item.Rt);
            }
            else
            {
                return new Peak2D(0, mzRange.LockValue, item.Rt); ;
            }
        }

        private static Peak2D[] GetMassTrace(
            IMzLiteArray<RtIndexerItem> index,
            IndexRange range,
            IMzLiteDataReader dataReader,
            RangeQuery mzRange,
            bool getLockMz)
        {
            Peak2D[] mt = new Peak2D[range.Length];

            int idx = 0;

            for (int i = range.Low; i <= range.Heigh; i++)
            {
                RtIndexerItem item = index[i];
                mt[idx] = GetClosestMz(dataReader, item, mzRange, getLockMz);                
                idx++;
            }

            return mt;
        }

        private static int SearchCompare(RtIndexerItem item, RangeQuery rtRange)
        {
            if (item.Rt < rtRange.LowValue)
                return -1;

            if (item.Rt > rtRange.HighValue)
                return 1;

            return 0;
        }

        private static int SearchCompare(Peak1D p, RangeQuery mzRange)
        {
            if (p.Mz < mzRange.LowValue)
                return -1;
            if (p.Mz > mzRange.HighValue)
                return 1;
            return 0;
        }

        private static IEnumerable<RtIndexerItem> Scan(IEnumerable<MassSpectrum> spectra, int msLevel)
        {

            RtIndexerItem item;

            foreach (var ms in spectra)
            {
                if (TryCreateSpectrumKey(ms, msLevel, out item))
                    yield return item;
            }
        }

        private static bool TryCreateSpectrumKey(MassSpectrum ms, int msLevel, out RtIndexerItem item)
        {

            item = null;
            int _msLevel;

            if (!ms.TryGetMsLevel(out _msLevel) || _msLevel != msLevel)
                return false;

            if (ms.Scans.Count < 1)
                return false;

            double rt;
            Scan scan = ms.Scans[0];

            if (scan.TryGetScanStartTime(out rt))
            {
                item = new RtIndexerItem(ms.ID, rt);
                return true;
            }
            else
            {
                return false;
            }
        }

        private class RtIndexerSortingComparer : IComparer<RtIndexerItem>
        {

            #region IComparer<RtIndexerItem> Members

            int IComparer<RtIndexerItem>.Compare(RtIndexerItem x, RtIndexerItem y)
            {
                return x.Rt.CompareTo(y.Rt);
            }

            #endregion
        }

        private struct ItemIndex<T> 
        {

            internal ItemIndex(T item, int idx) 
                : this()
            {
                Item = item;
                Index = idx;
            }

            internal T Item;
            internal int Index;
        }
    }

    public sealed class RtIndexerItem
    {

        private readonly string spectrumID;
        private readonly double rt;

        internal RtIndexerItem(string spectrumID, double rt)
        {
            this.spectrumID = spectrumID;
            this.rt = rt;
        }

        internal string SpectrumID { get { return spectrumID; } }
        internal double Rt { get { return rt; } }        
    }

    public sealed class RtIndexerQuery
    {

        private readonly RangeQuery rtRange;
        private readonly RangeQuery mzRange;

        public RtIndexerQuery(RangeQuery rtRange, RangeQuery mzRange)
        {

            if (mzRange == null)
                throw new ArgumentNullException("mzRange");
            if (rtRange == null)
                throw new ArgumentNullException("rtRange");

            this.rtRange = rtRange;
            this.mzRange = mzRange;
        }

        public RangeQuery RtRange { get { return rtRange; } }
        public RangeQuery MzRange { get { return mzRange; } }

    }
}
