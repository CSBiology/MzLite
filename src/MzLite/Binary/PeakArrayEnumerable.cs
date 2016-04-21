using System;
using System.Collections.Generic;
using MzLite.Model;

namespace MzLite.Binary
{
    public sealed class PeakArrayEnumerable : IPeakEnumerable, IEnumerator<IPeak>
    {

        private readonly IPeak[] peakArray;        
        private readonly PeakType peakType;        
        private int current = -1;

        public PeakArrayEnumerable(IPeak1D[] peakArray) 
            : this(peakArray, PeakType.Peak1D)
        {            
        }

        public PeakArrayEnumerable(IPeak2D[] peakArray)
            : this(peakArray, PeakType.Peak2D)
        {            
        }

        private PeakArrayEnumerable(IPeak[] peakArray, PeakType peakType)
        {
            if (peakArray == null)
                throw new ArgumentNullException("peakArray");
            this.peakArray = peakArray;
            this.peakType = peakType;
        }

        public PeakType PeakType { get { return peakType; } }

        #region IEnumerable<TValue> Members

        public IEnumerator<IPeak> GetEnumerator()
        {
            return new PeakArrayEnumerable(peakArray, peakType);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IArrayEnumerable<TValue> Members

        public int ArrayLength
        {
            get { return peakArray.Length; }
        }

        public IPeak this[int idx]
        {
            get
            {
                if (peakType == PeakType.Peak1D)
                    return peakArray[idx].AsPeak1D;
                else
                    return peakArray[idx].AsPeak2D;
            }
        }

        #endregion

        #region IEnumerator<IPeak> Members

        IPeak IEnumerator<IPeak>.Current
        {
            get { return this[current]; }
        }

        #endregion

        #region IDisposable Members

        void IDisposable.Dispose()
        {
        }

        #endregion

        #region IEnumerator Members

        object System.Collections.IEnumerator.Current
        {
            get { return this[current]; }
        }

        bool System.Collections.IEnumerator.MoveNext()
        {
            current += 1;
            return current < peakArray.Length;
        }

        void System.Collections.IEnumerator.Reset()
        {
            current = -1;
        }

        #endregion
    }
}
