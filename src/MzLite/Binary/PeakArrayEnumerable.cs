using System;
using System.Collections.Generic;
using MzLite.Model;

namespace MzLite.Binary
{
    public sealed class PeakArrayEnumerable<TPeak>
        : IPeakEnumerable<TPeak>, IEnumerator<TPeak>
        where TPeak : IPeak
    {

        private readonly TPeak[] peakArray;
        private int current = -1;

        public PeakArrayEnumerable(TPeak[] peakArray)
        {
            if (peakArray == null)
                throw new ArgumentNullException("peakArray");
            this.peakArray = peakArray;
        }

        #region IEnumerable<TPeak> Members

        public IEnumerator<TPeak> GetEnumerator()
        {
            return new PeakArrayEnumerable<TPeak>(peakArray);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IArrayEnumerable<TPeak> Members

        public int ArrayLength
        {
            get { return peakArray.Length; }
        }

        public TPeak this[int idx]
        {
            get
            {
                return peakArray[idx];
            }
        }

        #endregion

        #region IEnumerator<TPeak> Members

        TPeak IEnumerator<TPeak>.Current
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
