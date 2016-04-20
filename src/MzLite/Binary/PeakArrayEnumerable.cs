using System;
using System.Collections.Generic;
using MzLite.Model;

namespace MzLite.Binary
{
    public sealed class PeakArrayEnumerable : IPeakEnumerable, IEnumerator<IPeak>
    {

        private readonly double[] intArray;
        private readonly double[] mzArray;
        private readonly double[] rtArray;
        private readonly PeakType peakType;
        private readonly int arrayLength;
        private int current = -1;

        public PeakArrayEnumerable(double[] intArray, double[] mzArray)
        {
            if (intArray == null)
                throw new ArgumentNullException("intArray");
            if (mzArray == null)
                throw new ArgumentNullException("mzArray");
            this.intArray = intArray;
            this.mzArray = mzArray;
            this.peakType = PeakType.Peak1D;
            this.arrayLength = Math.Min(intArray.Length, mzArray.Length);
        }

        public PeakArrayEnumerable(double[] intArray, double[] mzArray, double[] rtArray)
        {
            if (intArray == null)
                throw new ArgumentNullException("intArray");
            if (mzArray == null)
                throw new ArgumentNullException("mzArray");
            if (rtArray == null)
                throw new ArgumentNullException("rtArray");
            this.intArray = intArray;
            this.mzArray = mzArray;
            this.rtArray = rtArray;
            this.peakType = PeakType.Peak2D;
            this.arrayLength = Math.Min(Math.Min(intArray.Length, mzArray.Length), rtArray.Length);
        }

        public PeakType PeakType { get { return peakType; } }

        #region IEnumerable<TValue> Members

        public IEnumerator<IPeak> GetEnumerator()
        {
            if (peakType == PeakType.Peak1D)
                return new PeakArrayEnumerable(intArray, mzArray);
            else
                return new PeakArrayEnumerable(intArray, mzArray, rtArray);
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
            get { return arrayLength; }
        }

        public IPeak this[int idx]
        {
            get
            {
                if (peakType == PeakType.Peak1D)
                    return new Peak1D(intArray[idx], mzArray[idx]);
                else
                    return new Peak2D(intArray[idx], mzArray[idx], rtArray[idx]);
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
            return current < arrayLength;
        }

        void System.Collections.IEnumerator.Reset()
        {
            current = -1;
        }

        #endregion
    }
}
