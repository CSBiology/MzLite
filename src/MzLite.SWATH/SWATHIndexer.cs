using System;
using System.Collections.Generic;
using MzLite.IO;
using MzLite.MetaData;
using MzLite.Model;

namespace MzLite.SWATH
{

    public sealed class SWATHIndexerItem
    {
        public double Mz { get; internal set; }
        public double MzStart { get; internal set; }
        public double MzEnd { get; internal set; }
        public double Rt { get; internal set; }
        public string SpectrumID { get; internal set; }

    }

    public delegate int SearchComparer(SWATHIndexerItem item, double mz, double rt, double rtOffset);

    public sealed class SWATHIndexer : IDisposable
    {

        private SWATHIndexerItem[] items;

        class ItemSorting : IComparer<SWATHIndexerItem>
        {

            #region IComparer<Item> Members

            public int Compare(SWATHIndexerItem x, SWATHIndexerItem y)
            {
                if (x.MzStart.CompareTo(y.MzStart) != 0)
                {
                    return x.MzStart.CompareTo(y.MzStart);
                }
                if (x.MzEnd.CompareTo(y.MzEnd) != 0)
                {
                    return x.MzEnd.CompareTo(y.MzEnd);
                }
                if (x.Rt.CompareTo(y.Rt) != 0)
                {
                    return x.Rt.CompareTo(y.Rt);
                }

                return 0;
            }

            #endregion
        }

        private SWATHIndexer() { }

        public static SWATHIndexer Build(IMzLiteDataReader reader, string runID)
        {
            return Build(reader, runID, CreateItem);
        }

        public static SWATHIndexer Build(IMzLiteDataReader reader, string runID, Func<MassSpectrum, SWATHIndexerItem> createItem)
        {

            IList<SWATHIndexerItem> list = new List<SWATHIndexerItem>(65000);

            foreach (MassSpectrum ms in reader.ReadMassSpectra(runID))
            {
                SWATHIndexerItem item = createItem.Invoke(ms);

                if (item != null)
                    list.Add(item);
            }

            SWATHIndexer indexer = new SWATHIndexer();
            indexer.items = new SWATHIndexerItem[list.Count];
            list.CopyTo(indexer.items, 0);

            Array.Sort(indexer.items, new ItemSorting());

            return indexer;
        }

        public IList<SWATHIndexerItem> Find(double mz, double rt, double rtOffset)
        {
            IList<SWATHIndexerItem> results = new List<SWATHIndexerItem>();
            return Find(Compare, mz, rt, rtOffset);
        }

        public IList<SWATHIndexerItem> Find(SearchComparer comparer, double mz, double rt, double rtOffset)
        {
            IList<SWATHIndexerItem> results = new List<SWATHIndexerItem>();
            return Find(results, comparer, mz, rt, rtOffset);
        }

        public IList<SWATHIndexerItem> Find(IList<SWATHIndexerItem> results, double mz, double rt, double rtOffset)
        {
            Find(results, items, Compare, 0, items.Length - 1, mz, rt, rtOffset);
            return results;
        }

        public IList<SWATHIndexerItem> Find(IList<SWATHIndexerItem> results, SearchComparer comparer, double mz, double rt, double rtOffset)
        {
            Find(results, items, comparer, 0, items.Length - 1, mz, rt, rtOffset);
            return results;
        }

        private static void Find(
            IList<SWATHIndexerItem> results, 
            SWATHIndexerItem[] items, 
            SearchComparer comparer,
            int lo, 
            int hi, 
            double mz, 
            double rt, 
            double rtOffset)
        {

            //for (int i = lo; i <= hi; i++)
            //{
            //    int c = comparer.Invoke(items[i], mz, rt, rtOffset);

            //    if (c == 0)
            //    {
            //        results.Add(items[i]);
            //    }
            //}


            while (lo <= hi)
            {
                int mid = lo + ((hi - lo) >> 1);

                int c = comparer.Invoke(items[mid], mz, rt, rtOffset);

                if (c == 0)
                {
                    results.Add(items[mid]);
                    Find(results, items, comparer, lo, mid - 1, mz, rt, rtOffset);
                    Find(results, items, comparer, mid + 1, hi, mz, rt, rtOffset);
                    return;
                }

                if (c < 0)
                {
                    lo = mid + 1;
                }
                else
                {
                    hi = mid - 1;
                }
            }

        }

        private static int Compare(SWATHIndexerItem item, double mz, double rt, double rtOffset)
        {
            if (item.MzEnd < mz)
                return -1;

            if (item.MzStart > mz)
                return 1;

            double lowRt = SaveSubtract(rt, rtOffset);

            if (item.Rt < lowRt)
                return -1;

            double heighRt = SaveAdd(rt, rtOffset);

            if (item.Rt > heighRt)
                return 1;

            return 0;
        }

        private static double SaveAdd(double x, double y)
        {
            decimal dx = new decimal(x);
            decimal dy = new decimal(y);
            return decimal.ToDouble(decimal.Add(dx, dy));
        }

        private static double SaveSubtract(double x, double y)
        {
            decimal dx = new decimal(x);
            decimal dy = new decimal(y);
            return decimal.ToDouble(decimal.Subtract(dx, dy));
        }

        private static SWATHIndexerItem CreateItem(MassSpectrum ms)
        {

            if (ms.BeginParamEdit().Get_MS_Level().GetInt32() != 2)
                return null;

            if (ms.Precursors.Count < 1 ||
                ms.Precursors[0].SelectedIons.Count < 1 ||
                ms.Scans.Count < 1)
                return null;

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

                double mzValue = mz.GetDouble().Value;

                return new SWATHIndexerItem()
                {
                    Mz = mz.GetDouble().Value,
                    MzStart = SaveSubtract(mzValue, mzLow.GetDouble().Value),
                    MzEnd = SaveAdd(mzValue, mzHigh.GetDouble().Value),
                    Rt = rt.GetDouble().Value,
                    SpectrumID = ms.ID
                };
            }
            else
            {
                return null;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            items = null;
        }

        #endregion
    }
}
