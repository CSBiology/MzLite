using System;
using System.IO;
using Clearcore2.Data.AnalystDataProvider;
using Clearcore2.Data.DataAccess.SampleData;
using MzLite.Binary;

namespace MzLite.Wiff
{
    public class WiffFileReader : IDisposable
    {

        private AnalystWiffDataProvider dataProvider;
        private Batch batch;
        private bool disposed = false;

        public WiffFileReader(string path)
            : this(path, GetUserLocalWiffLicense())
        {
        }

        public WiffFileReader(string wiffFilePath, string licenseFilePath)
        {

            if (string.IsNullOrWhiteSpace(wiffFilePath))
                throw new ArgumentNullException("wiffFilePath");
            if (string.IsNullOrWhiteSpace(licenseFilePath))
                throw new ArgumentNullException("licenseFilePath");
            if (!File.Exists(wiffFilePath))
                throw new FileNotFoundException("Wiff file not exists.");
            
            ReadWiffLicense(licenseFilePath);

            dataProvider = new AnalystWiffDataProvider(true);
            batch = AnalystDataProviderFactory.CreateBatch(wiffFilePath, dataProvider);                                    
        }

        #region wiff helper

        private static string GetUserLocalWiffLicense()
        {

            string appFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string wiffFolder = Path.Combine(appFolder, @"IOMIQS\Clearcore2\Licensing");
            if (!Directory.Exists(wiffFolder))
                Directory.CreateDirectory(wiffFolder);
            return Path.Combine(wiffFolder, "Clearcore2.license.xml");
        }

        private static void ReadWiffLicense(string licensePath)
        {
            if (!File.Exists(licensePath))
                throw new FileNotFoundException("Missing Clearcore2 license file: " + licensePath);
            string text = File.ReadAllText(licensePath);
            Clearcore2.Licensing.LicenseKeys.Keys = new[] { text };
        }

        private static IPeakEnumerable GetPeaks(
            Batch batch,
            int sampleIndex,
            int experimentIndex,
            int scanIndex)
        {
            Clearcore2.Data.DataAccess.SampleData.Sample sample = batch.GetSample(sampleIndex);
            MassSpectrometerSample msSample = sample.MassSpectrometerSample;
            MSExperiment msExp = msSample.GetMSExperiment(experimentIndex);
            return new WiffPeakEnumerable(msExp.GetMassSpectrum(scanIndex));
        }

        private static MzLite.Model.MassSpectrum GetFeature(
            Batch batch,
            int sampleIndex,
            int experimentIndex,
            int scanIndex)
        {
            Clearcore2.Data.DataAccess.SampleData.Sample sample = batch.GetSample(sampleIndex);
            MassSpectrometerSample msSample = sample.MassSpectrometerSample;
            MSExperiment msExp = msSample.GetMSExperiment(experimentIndex);
            Clearcore2.Data.MassSpectrumInfo wiffSpectrum = msExp.GetMassSpectrumInfo(scanIndex);

            // create a spectrum name 
            string nativeID = string.Format("sample={0} period={1} cycle={2} experiment={3}", 
                sampleIndex,
                wiffSpectrum.PeriodIndex,
                wiffSpectrum.StartCycle,
                experimentIndex);

            MzLite.Model.MassSpectrum mzLiteSpectrum = new Model.MassSpectrum(nativeID);

            // setup default peak data encodings
            mzLiteSpectrum.PeakArray.IntensityDataType = Model.BinaryDataType.Float32;
            mzLiteSpectrum.PeakArray.MzDataType = Model.BinaryDataType.Float64;
            mzLiteSpectrum.PeakArray.CompressionType = Model.BinaryDataCompressionType.ZLib;

            return mzLiteSpectrum;
        }

        #endregion

        #region IDisposable Members

        private void RaiseDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(this.GetType().Name);
        }

        public void Dispose()
        {
            if (disposed)
                return;

            if (dataProvider != null)
                dataProvider.Close();

            batch = null;

            disposed = true;
        }

        #endregion
    }
}
