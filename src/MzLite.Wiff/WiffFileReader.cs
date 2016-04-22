using System;
using System.IO;
using Clearcore2.Data.AnalystDataProvider;
using Clearcore2.Data.DataAccess.SampleData;
using MzLite.Binary;
using MzLite.Model;
using MzLite.MetaData;
using MzLite.IO;

namespace MzLite.Wiff
{
    public class WiffFileReader : IDisposable
    {

        private AnalystWiffDataProvider dataProvider;
        private Batch batch;
        private bool disposed = false;
        private string wiffFilePath;

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
            this.wiffFilePath = wiffFilePath;
            dataProvider = new AnalystWiffDataProvider(true);
            batch = AnalystDataProviderFactory.CreateBatch(wiffFilePath, dataProvider);                                    
        }

        public MzLiteProject ReadProject()
        {
            return CreateProject(batch, wiffFilePath);
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

        private static MzLite.Model.MassSpectrum GetSpectrum(
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

            // <---------- description --------------------------->

            // spectrum

            IParamEdit paramEdit = mzLiteSpectrum.BeginParamEdit();

            paramEdit.MS_MsLevel(wiffSpectrum.MSLevel);

            if (wiffSpectrum.CentroidMode)
                paramEdit.MS_CentroidSpectrum();
            else
                paramEdit.MS_ProfileSpectrum();

            // scan

            Scan scan = new Scan();
            scan.BeginParamEdit()
                .MS_ScanStartTime(wiffSpectrum.StartRT)
                .UO_Minute();

            mzLiteSpectrum.Scans.Add(scan);

            // precursor

            if (wiffSpectrum.IsProductSpectrum)
            {

                Precursor precursor = new Precursor();

                double isoWidth;
                double targetMz;

                if (GetIsolationWindow(wiffSpectrum.Experiment, out isoWidth, out targetMz))
                {
                    precursor.IsolationWindow.BeginParamEdit()
                        .MS_IsolationWindowTargetMz(targetMz)
                        .MS_IsolationWindowUpperOffset(isoWidth)
                        .MS_IsolationWindowLowerOffset(isoWidth);
                }

                SelectedIon selectedIon = new SelectedIon();

                selectedIon.BeginParamEdit()
                    .MS_SelectedIonMz(wiffSpectrum.ParentMZ)
                    .MS_ChargeState(wiffSpectrum.ParentChargeState);

                precursor.SelectedIons.Add(selectedIon);

                precursor.Activation.BeginParamEdit()
                    .MS_CollisionEnergy(wiffSpectrum.CollisionEnergy);

                mzLiteSpectrum.Precursors.Add(precursor);
            }

            return mzLiteSpectrum;
        }

        private static bool GetIsolationWindow(
            MSExperiment exp,
            out double isoWidth,
            out double targetMz)
        {
            FragmentBasedScanMassRange mri = null;
            MassRange[] mr = exp.Details.MassRangeInfo;
            isoWidth = 0d;
            targetMz = 0d;

            if (mr.Length > 0)
            {
                mri = mr[0] as FragmentBasedScanMassRange;
                isoWidth = mri.IsolationWindow * 0.5d;
                targetMz = mri.FixedMasses[0];
            }

            return mri != null;
        }
        
        private static MzLiteProject CreateProject(Batch batch, string wiffFilePath)
        {
            MzLiteProject project = new MzLiteProject(batch.Name);

            DataFile dataFile = new DataFile(Path.GetFileName(wiffFilePath), wiffFilePath);
            dataFile.BeginParamEdit().MS_ABIWIFFFormat();
            project.DataFiles.Add(dataFile);

            string[] sampleNames = batch.GetSampleNames();

            for (int sampleIdx = 0; sampleIdx < sampleNames.Length; sampleIdx++)
            {
                using (Clearcore2.Data.DataAccess.SampleData.Sample wiffSample = batch.GetSample(sampleIdx))
                {
                    if (wiffSample.HasMassSpectrometerData)
                    {
                        string sampleName = sampleNames[sampleIdx];
                        MassSpectrometerSample msSample = wiffSample.MassSpectrometerSample;
                        MzLite.Model.Sample mzLiteSample = new MzLite.Model.Sample(sampleName);
                        project.Samples.Add(mzLiteSample);

                        Instrument instrument = new Instrument(msSample.InstrumentName);
                        Software software = new Software(wiffSample.Details.SoftwareVersion);
                        project.Software.Add(software);
                        instrument.Software = software;
                        project.Instruments.Add(instrument);

                        for (int expIdx = 0; expIdx < msSample.ExperimentCount; expIdx++)
                        {
                            using (MSExperiment msExp = msSample.GetMSExperiment(expIdx))
                            {
                                string runName = string.Format("[{0}].[{1}]", sampleName, msExp.Details.Name);
                                Run run = new Run(runName);
                                run.Sample = mzLiteSample;
                                run.Instrument = instrument;
                                run.DataFile = dataFile;
                                project.Runs.Add(run);
                            }
                        }
                    }                    
                }
            }

            return project;
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
