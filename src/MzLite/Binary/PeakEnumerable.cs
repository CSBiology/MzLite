using System.Collections.Generic;
using MzLite.Model;

namespace MzLite.Binary
{
    public interface IPeakEnumerable : IEnumerable<IPeak>
    {
        int ArrayLength { get; }
        IPeak this[int idx] { get; }
    }

    //public class Peak1DEnumerable : IPeakEnumerable<Peak1D>
    //{

    //    private readonly TValue[] array;

    //    public Peak1DEnumerable(TValue[] array)
    //    {
    //        if (array == null)
    //            throw new ArgumentNullException("array");
    //        this.array = array;
    //    }


    //    #region IEnumerable<TValue> Members

    //    public IEnumerator<TValue> GetEnumerator()
    //    {
    //        IEnumerable<TValue> ie = array;
    //        return ie.GetEnumerator();
    //    }

    //    #endregion

    //    #region IEnumerable Members

    //    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    //    {
    //        return array.GetEnumerator();
    //    }

    //    #endregion

    //    #region IArrayEnumerable<TValue> Members

    //    public int ArrayLength
    //    {
    //        get { return array.Length; }
    //    }

    //    public TValue this[int idx]
    //    {
    //        get { return array[idx]; }
    //    }

    //    #endregion
    //}
}
