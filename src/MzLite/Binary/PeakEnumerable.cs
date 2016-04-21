using System.Collections.Generic;
using MzLite.Model;

namespace MzLite.Binary
{
    public interface IPeakEnumerable : IEnumerable<IPeak>
    {
        int ArrayLength { get; }
        IPeak this[int idx] { get; }
        PeakType PeakType { get; }
    }

    
}
