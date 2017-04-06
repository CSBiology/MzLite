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

namespace MzLite.Processing
{
   
    /// <summary>
    /// Supports indexing of mass spectra by retention time.
    /// </summary>
    public sealed class RtIndexer
    {

        private readonly KeyValuePair<double, string>[] entries;
        private static readonly Peak2D[] empty2D = new Peak2D[0];

        private RtIndexer(KeyValuePair<double, string>[] entries)
        {
            this.entries = entries;
        }

        /// <summary>
        /// Create an rt index for spectra of specified run and ms level.
        /// </summary>        
        public static RtIndexer Create(
            IMzLiteDataReader dataReader, 
            string runID, 
            int msLevel)
        {
            if (dataReader == null)
                throw new ArgumentNullException("dataReader");
            if(string.IsNullOrWhiteSpace(runID))
                throw new ArgumentNullException("runID");

            IEnumerable<MassSpectrum> massSpectra = dataReader.ReadMassSpectra(runID);
            KeyValuePair<double, string>[] rtSpectra = Entries(massSpectra, msLevel).ToArray();
            Array.Sort(rtSpectra, new RtIndexerSortingComparer());
            return new RtIndexer(rtSpectra);
        }
        
        /// <summary>
        /// Extract a Peak2D list over the retention time range specified in the query rt range.
        /// </summary>
        /// <param name="dataReader">The reader to read the spectra from.</param>
        /// <param name="query">The query specify rt and mz ranges to search.</param>
        /// <param name="mzRangeSelector">A function to select a peak within mz range for each spectrum.</param>      
        public Peak2D[] GetMassTrace(
            IMzLiteDataReader dataReader,
            RtIndexerQuery query,            
            Func<IEnumerable<Peak1D>, RangeQuery, Peak1D> mzRangeSelector = null)
        {
            if (dataReader == null)
                throw new ArgumentNullException("dataReader");
            if (query == null)
                throw new ArgumentNullException("query");
            if (mzRangeSelector == null)
                mzRangeSelector = GetClosestMz;

            IndexRange range;

            if (BinarySearch.Search(entries, query.RtRange, RtRangeCompare, out range))
            {
                Peak2D[] mt = new Peak2D[range.Length];

                int idx = 0;

                for (int i = range.Low; i <= range.Heigh; i++)
                {
                    KeyValuePair<double, string> entry = entries[i];
                    Peak1DArray spectrumPeaks = dataReader.ReadSpectrumPeaks(entry.Value);
                    IEnumerable<Peak1D> mzPeaks = BinarySearch.Search(spectrumPeaks.Peaks, query.MzRange, MzRangeCompare);
                    Peak1D p = mzRangeSelector(mzPeaks, query.MzRange);
                    mt[idx] = new Peak2D(p.Intensity, p.Mz, entry.Key);
                    idx++;
                }

                return mt;
            }
            else
            {
                return empty2D;
            }
        
        }

        /// <summary>
        /// The default mz range peak selector function.
        /// </summary>        
        private static Peak1D GetClosestMz(IEnumerable<Peak1D> peaks, RangeQuery mzRange)
        {
            return peaks
                .DefaultIfEmpty(new Peak1D(0, mzRange.LockValue))
                .ItemAtMin(x => Math.Abs(x.Mz - mzRange.LockValue));
        }        

        private static int RtRangeCompare(KeyValuePair<double, string> item, RangeQuery rtRange)
        {
            if (item.Key < rtRange.LowValue)
                return -1;

            if (item.Key > rtRange.HighValue)
                return 1;

            return 0;
        }

        private static int MzRangeCompare(Peak1D p, RangeQuery mzRange)
        {
            if (p.Mz < mzRange.LowValue)
                return -1;
            if (p.Mz > mzRange.HighValue)
                return 1;
            return 0;
        }

        private static IEnumerable<KeyValuePair<double, string>> Entries(IEnumerable<MassSpectrum> spectra, int msLevel)
        {

            KeyValuePair<double, string> item;

            foreach (var ms in spectra)
            {
                if (TryCreateEntry(ms, msLevel, out item))
                    yield return item;
            }
        }

        private static bool TryCreateEntry(MassSpectrum ms, int msLevel, out KeyValuePair<double, string> entry)
        {
            entry = default(KeyValuePair<double, string>);
            int _msLevel;

            if (!ms.TryGetMsLevel(out _msLevel) || _msLevel != msLevel)
                return false;

            if (ms.Scans.Count < 1)
                return false;

            double rt;
            Scan scan = ms.Scans[0];

            if (scan.TryGetScanStartTime(out rt))
            {
                entry = new KeyValuePair<double, string>(rt, ms.ID);
                return true;
            }
            else
            {
                return false;
            }
        }

        private class RtIndexerSortingComparer : IComparer<KeyValuePair<double, string>>
        {

            #region IComparer<RtIndexerItem> Members

            int IComparer<KeyValuePair<double, string>>.Compare(KeyValuePair<double, string> x, KeyValuePair<double, string> y)
            {
                return x.Key.CompareTo(y.Key);
            }

            #endregion
        }        
    }
    
    /// <summary>
    /// Represents the rt index query data structure.  
    /// </summary>
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
