using System.Collections;
using System.Collections.Generic;
using MzLite.Model;

namespace MzLite.Binary
{

    public interface IPeakEnumerable : IEnumerable
    {
        int ArrayLength { get; }
    }

    public interface IPeakEnumerable<TPeak> : IEnumerable<TPeak> 
        where TPeak : IPeak
    {
        int ArrayLength { get; }
        TPeak this[int idx] { get; }        
    }

    
}
