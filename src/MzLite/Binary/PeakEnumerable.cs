using System.Collections.Generic;
using MzLite.Model;

namespace MzLite.Binary
{
    public interface IPeakEnumerable<TPeak> : IEnumerable<TPeak> where TPeak : IPeak
    {
        int ArrayLength { get; }
        TPeak this[int idx] { get; }
        PeakType PeakType { get; }
    }

    
}
