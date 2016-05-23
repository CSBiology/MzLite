using System.Collections.Generic;
using MzLite.Binary;
using MzLite.Model;

namespace MzLite.IO
{
    public interface IMzLiteDataReader : IMzLiteIO
    {

        IEnumerable<MassSpectrum> ReadMassSpectra(string runID);
        MassSpectrum ReadMassSpectrum(string spectrumID);
        Peak1DArray ReadSpectrumPeaks(string spectrumID);

        IEnumerable<Chromatogram> ReadChromatograms(string runID);
        Chromatogram ReadChromatogram(string chromatogramID);
        Peak2DArray ReadChromatogramPeaks(string chromatogramID);
    }
}
