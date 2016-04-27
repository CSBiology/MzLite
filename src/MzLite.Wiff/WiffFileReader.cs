using System;
using System.IO;
using Clearcore2.Data.AnalystDataProvider;
using Clearcore2.Data.DataAccess.SampleData;
using MzLite.Binary;
using MzLite.Model;
using MzLite.MetaData;
using Clearcore2.Data;
using MzLite.IO;
using MzLite.Json;
using System.Collections.Generic;

namespace MzLite.Wiff
{
    public class WiffFileReader : IDisposable
    {

        private readonly AnalystWiffDataProvider dataProvider;
        private readonly Batch batch;
        private bool disposed = false;                
        private readonly string wiffFilePath;
        private readonly MzLiteModel model;

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
            try
            {
                ReadWiffLicense(licenseFilePath);

                this.dataProvider = new AnalystWiffDataProvider(true);
                this.batch = AnalystDataProviderFactory.CreateBatch(wiffFilePath, dataProvider);
                this.wiffFilePath = wiffFilePath;

                if (!File.Exists(GetModelFilePath(wiffFilePath)))
                {
                    model = CreateModel(batch, wiffFilePath);
                    MzLiteJson.SaveModel(model, GetModelFilePath(wiffFilePath));
                }
                else
                {
                    model = MzLiteJson.LoadModel(GetModelFilePath(wiffFilePath));
                }
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException(ex.Message, ex);
            }
        }                       

        public MzLiteModel GetModel()
        {
            return model;
        }

        public void SaveModel()
        {
            MzLiteJson.SaveModel(model, GetModelFilePath(wiffFilePath));
        }

        public WiffRunReader GetRunReader(string runID)
        {
            int sampleIndex = WiffNativeID.ParseWiffSampleIndex(runID);
            return new WiffRunReader(batch, sampleIndex);
        }

        #region WiffFileReader Members

        private static string GetModelFilePath(string wiffFilePath)
        {
            return wiffFilePath + ".mzlitemodel";
        }

        private static MzLiteModel CreateModel(Batch batch, string wiffFilePath)
        {
            MzLiteModel model = new MzLiteModel(batch.Name);
            
            string[] sampleNames = batch.GetSampleNames();

            for (int sampleIdx = 0; sampleIdx < sampleNames.Length; sampleIdx++)
            {
                using (Clearcore2.Data.DataAccess.SampleData.Sample wiffSample = batch.GetSample(sampleIdx))
                {
                    if (wiffSample.HasMassSpectrometerData)
                    {
                        string sampleName = sampleNames[sampleIdx].Trim();
                        string sampleID = string.Format("sample={0}", sampleIdx);
                        MassSpectrometerSample msSample = wiffSample.MassSpectrometerSample;
                        MzLite.Model.Sample mzLiteSample = new MzLite.Model.Sample(
                            sampleID,
                            sampleName);
                        model.Samples.Add(mzLiteSample);

                        string softwareID = wiffSample.Details.SoftwareVersion.Trim();
                        Software software = null;

                        if (!model.Software.TryGetItemByKey(softwareID, out software))
                        {
                            software = new Software(softwareID);
                            model.Software.Add(software);
                        }

                        string instrumentID = msSample.InstrumentName.Trim();
                        Instrument instrument = null;

                        if (!model.Instruments.TryGetItemByKey(instrumentID, out instrument))
                        {
                            instrument = new Instrument(instrumentID);
                            instrument.Software = software;
                            model.Instruments.Add(instrument);
                        }

                        string runID = string.Format("sample={0}", sampleIdx);

                        Run run = new Run(runID);
                        run.Sample = mzLiteSample;
                        run.DefaultInstrument = instrument;
                        model.Runs.Add(run);
                    }
                }
            }

            return model;
        } 

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

            disposed = true;
        }

        #endregion
    }

    public class WiffRunReader : IDisposable
    {

        private readonly MassSpectrometerSample sample;
        private readonly int sampleIndex;
        private bool disposed = false;

        internal WiffRunReader(Batch batch, int sampleIndex)
        {
            this.sample = batch.GetSample(sampleIndex).MassSpectrometerSample;
            this.sampleIndex = sampleIndex;
        }

        public IEnumerable<MzLite.Model.MassSpectrum> ReadMassSpectra()
        {
            foreach(var id in YieldIDs())
                yield return GetSpectrum(id);
        }

        public MzLite.Model.MassSpectrum ReadSpectrum(string spectrumID)
        {
            return GetSpectrum(WiffNativeID.Parse(spectrumID));
        }

        public IPeakEnumerable<IPeak1D> ReadSpectrumPeaks(string spectrumID)
        {
            return GetPeaks(WiffNativeID.Parse(spectrumID));
        }

        #region WiffRunReader Members

        private IEnumerable<WiffNativeID> YieldIDs()
        {
            for (int experimentIndex = 0; experimentIndex < sample.ExperimentCount; experimentIndex++)
            {
                MSExperiment exp = sample.GetMSExperiment(experimentIndex);
                for (int scanIndex = 0; scanIndex < exp.Details.NumberOfScans; scanIndex++)
                {
                    MassSpectrumInfo mi = exp.GetMassSpectrumInfo(scanIndex);
                    yield return new WiffNativeID(sampleIndex, mi.PeriodIndex, scanIndex, experimentIndex);
                }
            }
        }

        private MzLite.Model.MassSpectrum GetSpectrum(            
            WiffNativeID id)
        {
            using (MSExperiment msExp = sample.GetMSExperiment(id.Experiment))
            {
                MassSpectrumInfo wiffSpectrum = msExp.GetMassSpectrumInfo(id.Cycle);

                MzLite.Model.MassSpectrum mzLiteSpectrum = new Model.MassSpectrum(id.ToString());

                // setup default peak data encodings
                mzLiteSpectrum.PeakArray.IntensityDataType = BinaryDataType.Float32;
                mzLiteSpectrum.PeakArray.MzDataType = BinaryDataType.Float64;
                mzLiteSpectrum.PeakArray.CompressionType = BinaryDataCompressionType.ZLib;

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
        }

        private WiffPeakEnumerable GetPeaks(            
            WiffNativeID id)
        {
            using (MSExperiment msExp = sample.GetMSExperiment(id.Experiment))
            {
                return new WiffPeakEnumerable(msExp.GetMassSpectrum(id.Cycle));
            }
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
            sample.Dispose();
            disposed = true;
        }

        #endregion
    }
}
