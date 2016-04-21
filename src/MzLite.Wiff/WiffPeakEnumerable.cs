using System;
using System.Collections.Generic;
using MzLite.Binary;

namespace MzLite.Wiff
{
    public class WiffPeakEnumerable : IPeakEnumerable, IEnumerator<Model.IPeak>
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

        public Model.IPeak this[int idx]
        {
            get { return new Model.Peak1D(wiffSpectrum.GetXValue(idx), wiffSpectrum.GetYValue(idx)); }
        }

        public Model.PeakType PeakType
        {
            get { return Model.PeakType.Peak1D; }
        }

        #endregion

        #region IEnumerable<IPeak> Members

        public IEnumerator<Model.IPeak> GetEnumerator()
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

        Model.IPeak IEnumerator<Model.IPeak>.Current
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
