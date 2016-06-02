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
using System.Text.RegularExpressions;

namespace MzLite.Wiff
{
    public class WiffFileReader : IMzLiteDataReader
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
                    MzLiteJson.SaveJsonFile(model, GetModelFilePath(wiffFilePath));
                }
                else
                {
                    model = MzLiteJson.ReadJsonFile<MzLiteModel>(GetModelFilePath(wiffFilePath));
                }
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException(ex.Message, ex);
            }
        }

        #region IMzLiteDataReader Members

        public IEnumerable<MzLite.Model.MassSpectrum> ReadMassSpectra(string runID)
        {
            RaiseDisposed();            

            try
            {
                int sampleIndex;
                Parse(runID, out sampleIndex);
                return Yield(batch, sampleIndex);
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException(ex.Message, ex);
            }
        }

        public MzLite.Model.MassSpectrum ReadMassSpectrum(string spectrumID)
        {
            RaiseDisposed();            

            try
            {
                int sampleIndex, experimentIndex, scanIndex;
                Parse(spectrumID, out sampleIndex, out experimentIndex, out scanIndex);

                using (MassSpectrometerSample sample = batch.GetSample(sampleIndex).MassSpectrometerSample)
                using (MSExperiment msExp = sample.GetMSExperiment(experimentIndex))
                {
                    return GetSpectrum(batch, sample, msExp, sampleIndex, experimentIndex, scanIndex);
                }
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException(ex.Message, ex);
            }
        }

        public Peak1DArray ReadSpectrumPeaks(string spectrumID)
        {
            RaiseDisposed();            

            try
            {
                int sampleIndex, experimentIndex, scanIndex;
                Parse(spectrumID, out sampleIndex, out experimentIndex, out scanIndex);

                using (MassSpectrometerSample sample = batch.GetSample(sampleIndex).MassSpectrometerSample)
                using (MSExperiment msExp = sample.GetMSExperiment(experimentIndex))
                {
                    Clearcore2.Data.MassSpectrum ms = msExp.GetMassSpectrum(scanIndex);
                    Peak1DArray pa = new Peak1DArray(
                        ms.NumDataPoints,
                        BinaryDataCompressionType.ZLib,
                        BinaryDataType.Float32,
                        BinaryDataType.Float64);

                    for (int i = 0; i < ms.NumDataPoints; i++)
                    {
                        pa.Peaks[i] = new Peak1D(ms.GetYValue(i), ms.GetXValue(i));
                    }

                    return pa;
                }
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException(ex.Message, ex);
            }
        }

        public IEnumerable<Chromatogram> ReadChromatograms(string runID)
        {            
            try
            {
                throw new NotSupportedException();
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException(ex.Message, ex);
            }
        }

        public Chromatogram ReadChromatogram(string chromatogramID)
        {
            try
            {
                throw new NotSupportedException();
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException(ex.Message, ex);
            }
        }

        public Peak2DArray ReadChromatogramPeaks(string chromatogramID)
        {
            try
            {
                throw new NotSupportedException();
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException(ex.Message, ex);
            }
        }

        #endregion

        #region IMzLiteIO Members

        public MzLiteModel GetModel()
        {
            RaiseDisposed();
            return model;
        }

        public void SaveModel()
        {
            RaiseDisposed();
            
            try
            {
                MzLiteJson.SaveJsonFile(model, GetModelFilePath(wiffFilePath));
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException(ex.Message, ex);
            }
        }
        
        public ITransactionScope BeginTransaction()
        {
            return new WiffTransactionScope();
        }

        #endregion

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
                        string sampleID = ToRunID(sampleIdx);
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

        private static IEnumerable<MzLite.Model.MassSpectrum> Yield(Batch batch, int sampleIndex)
        {
            using (MassSpectrometerSample sample = batch.GetSample(sampleIndex).MassSpectrometerSample)
            {
                for (int experimentIndex = 0; experimentIndex < sample.ExperimentCount; experimentIndex++)
                {
                    using (MSExperiment msExp = sample.GetMSExperiment(experimentIndex))
                    {
                        for (int scanIndex = 0; scanIndex < msExp.Details.NumberOfScans; scanIndex++)
                        {
                            yield return GetSpectrum(batch, sample, msExp, sampleIndex, experimentIndex, scanIndex);
                        }
                    }
                }
            }
        }

        private static MzLite.Model.MassSpectrum GetSpectrum(
            Batch batch,
            MassSpectrometerSample sample,
            MSExperiment msExp,
            int sampleIndex,
            int experimentIndex,
            int scanIndex)
        {
            MassSpectrumInfo wiffSpectrum = msExp.GetMassSpectrumInfo(scanIndex);

            MzLite.Model.MassSpectrum mzLiteSpectrum = new Model.MassSpectrum(ToSpectrumID(sampleIndex, experimentIndex, scanIndex));
            
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

        static readonly decimal decHalf = new decimal(0.5d);

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
                isoWidth = decimal.ToDouble(decimal.Multiply( new decimal(mri.IsolationWindow), decHalf));
                targetMz = mri.FixedMasses[0];
            }

            return mri != null;
        }

        static readonly Regex regexID = new Regex(@"sample=(\d+) experiment=(\d+) scan=(\d+)", RegexOptions.Compiled | RegexOptions.ECMAScript);
        static readonly Regex regexSampleIndex = new Regex(@"sample=(\d+)", RegexOptions.Compiled | RegexOptions.ECMAScript);

        private static string ToSpectrumID(int sampleIndex, int experimentIndex, int scanIndex)
        {
            return string.Format("sample={0} experiment={1} scan={2}", sampleIndex, experimentIndex, scanIndex);            
        }

        private static string ToRunID(int sample)
        {
            return string.Format("sample={0}", sample);
        }

        private static void Parse(string runID, out int sampleIndex)
        {

            Match match = regexSampleIndex.Match(runID);

            if (match.Success)
            {
                try
                {
                    GroupCollection groups = match.Groups;
                    sampleIndex = int.Parse(groups[1].Value);
                }
                catch (Exception ex)
                {
                    throw new FormatException("Error parsing wiff sample index: " + runID, ex);
                }
            }
            else
            {
                throw new FormatException("Not a valid wiff sample index format: " + runID);
            }

        }

        private static void Parse(string spectrumID, out int sampleIndex, out int experimentIndex, out int scanIndex)
        {            
            Match match = regexID.Match(spectrumID);

            if (match.Success)
            {
                try
                {
                    GroupCollection groups = match.Groups;
                    sampleIndex = int.Parse(groups[1].Value);
                    experimentIndex = int.Parse(groups[2].Value);
                    scanIndex = int.Parse(groups[3].Value);                    
                }
                catch (Exception ex)
                {
                    throw new FormatException("Error parsing wiff spectrum id format: " + spectrumID, ex);
                }
            }
            else
            {
                throw new FormatException("Not a valid wiff spectrum id format: " + spectrumID);
            }

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

    internal class WiffTransactionScope : ITransactionScope
    {
        #region ITransactionScope Members

        public void Commit()
        {            
        }

        public void Rollback()
        {            
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {            
        }

        #endregion
    }
}
