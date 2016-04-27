using System;
using System.Collections.Generic;
using System.Diagnostics;
using MzLite.Binary;
using MzLite.Model;

namespace MzLite.Wiff
{
    public class WiffPeakEnumerable : IPeakEnumerable<IPeak1D>, IEnumerator<IPeak1D>
    {

        private Clearcore2.Data.MassSpectrum wiffSpectrum;
        private int current = -1;

        internal WiffPeakEnumerable(Clearcore2.Data.MassSpectrum wiffSpectrum)
        {
            this.wiffSpectrum = wiffSpectrum;
        }

        #region IPeakEnumerable Members

        public int ArrayLength
        {
            get { return wiffSpectrum.NumDataPoints; }
        }

        public IPeak1D this[int idx]
        {
            get { return new Peak1D(wiffSpectrum.GetYValue(idx), wiffSpectrum.GetXValue(idx)); }
        }

        public PeakType PeakType
        {
            get { return Model.PeakType.Peak1D; }
        }

        #endregion

        #region IEnumerable<IPeak1D> Members

        public IEnumerator<IPeak1D> GetEnumerator()
        {
            return new WiffPeakEnumerable(wiffSpectrum);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new WiffPeakEnumerable(wiffSpectrum);
        }

        #endregion

        #region IEnumerator<IPeak> Members

        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)]
        IPeak1D IEnumerator<IPeak1D>.Current
        {
            get { return this[current]; }
        }

        #endregion

        #region IDisposable Members

        bool isDisposed = false;

        void IDisposable.Dispose()
        {
            if (isDisposed)
                return;
            wiffSpectrum = null;
            isDisposed = true;
        }

        #endregion

        #region IEnumerator Members

        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)]
        object System.Collections.IEnumerator.Current
        {
            get { return this[current]; }
        }

        bool System.Collections.IEnumerator.MoveNext()
        {
            current += 1;
            return current < wiffSpectrum.NumDataPoints;
        }

        void System.Collections.IEnumerator.Reset()
        {
            current = -1;
        }

        #endregion
    }
}
