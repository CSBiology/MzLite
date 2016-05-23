using System;
using MzLite.Binary;
using MzLite.Model;

namespace MzLite.IO
{
    public interface IMzLiteDataWriter : IMzLiteIO
    {

        void Insert(string runID, MassSpectrum spectrum, Peak1DArray peaks);
        void Insert(string runID, Chromatogram chromatogram, Peak2DArray peaks);
    }
}
