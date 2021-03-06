﻿#region license
// The MIT License (MIT)

// wifffilereader.cs

// Copyright (c) 2016 Alexander Lüdemann
// alexander.luedemann@outlook.com
// luedeman@rhrk.uni-kl.de

// Computational Systems Biology, Technical University of Kaiserslautern, Germany


// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.IO;
using Clearcore2.Data.AnalystDataProvider;
using Clearcore2.Data.DataAccess.SampleData;
using MzLite.Binary;
using MzLite.Model;
using MzLite.MetaData.PSIMS;
using MzLite.MetaData.UO;
using Clearcore2.Data;
using MzLite.IO;
using MzLite.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using MzLite.Commons.Arrays;
using System.Threading.Tasks;

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
                this.model = MzLiteJson.HandleExternalModelFile(this, GetModelFilePath());
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
                        BinaryDataCompressionType.NoCompression,
                        BinaryDataType.Float32,
                        BinaryDataType.Float32);

                    //Peak1D[] peaks = new Peak1D[ms.NumDataPoints];

                    //for (int i = 0; i < ms.NumDataPoints; i++)
                    //    peaks[i] = new Peak1D(ms.GetYValue(i), ms.GetXValue(i));

                    //pa.Peaks = MzLiteArray.ToMzLiteArray(peaks);

                    pa.Peaks = new WiffPeaksArray(ms);

                    return pa;
                }
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException(ex.Message, ex);
            }

        }

        public Task<MzLite.Model.MassSpectrum> ReadMassSpectrumAsync(string spectrumID)
        {
            return Task<MzLite.Model.MassSpectrum>.Run(() => { return ReadMassSpectrum(spectrumID); });
        }

        public Task<Peak1DArray> ReadSpectrumPeaksAsync(string spectrumID)
        {
            return Task<Peak1DArray>.Run(() => { return ReadSpectrumPeaks(spectrumID); });
        }

        public IEnumerable<Chromatogram> ReadChromatograms(string runID)
        {
            return Enumerable.Empty<Chromatogram>();
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

        public Task<Chromatogram> ReadChromatogramAsync(string spectrumID)
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

        public Task<Peak2DArray> ReadChromatogramPeaksAsync(string spectrumID)
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

        public MzLiteModel CreateDefaultModel()
        {
            RaiseDisposed();

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

        public MzLiteModel Model
        {
            get
            {
                RaiseDisposed();
                return model;
            }
        }

        public void SaveModel()
        {
            RaiseDisposed();

            try
            {
                MzLiteJson.SaveJsonFile(model, GetModelFilePath());
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException(ex.Message, ex);
            }
        }

        public ITransactionScope BeginTransaction()
        {
            RaiseDisposed();
            return new WiffTransactionScope();
        }

        #endregion

        #region WiffFileReader Members

        private string GetModelFilePath()
        {
            return wiffFilePath + ".mzlitemodel";
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

            mzLiteSpectrum.SetMsLevel(wiffSpectrum.MSLevel);

            if (wiffSpectrum.CentroidMode)
                mzLiteSpectrum.SetCentroidSpectrum();
            else
                mzLiteSpectrum.SetProfileSpectrum();

            // scan

            Scan scan = new Scan();
            scan.SetScanStartTime(wiffSpectrum.StartRT)
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
                    precursor.IsolationWindow
                        .SetIsolationWindowTargetMz(targetMz)
                        .SetIsolationWindowUpperOffset(isoWidth)
                        .SetIsolationWindowLowerOffset(isoWidth);
                }

                SelectedIon selectedIon = new SelectedIon();

                selectedIon.SetSelectedIonMz(wiffSpectrum.ParentMZ)
                    .SetChargeState(wiffSpectrum.ParentChargeState);

                precursor.SelectedIons.Add(selectedIon);

                precursor.Activation
                    .SetCollisionEnergy(wiffSpectrum.CollisionEnergy);

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

    internal sealed class WiffPeaksArray : IMzLiteArray<Peak1D>
    {

        private readonly Clearcore2.Data.MassSpectrum wiffSpectrum;

        internal WiffPeaksArray(Clearcore2.Data.MassSpectrum wiffSpectrum)
        {
            this.wiffSpectrum = wiffSpectrum;
        }

        #region IMzLiteArray<Peak1D> Members

        public int Length
        {
            get { return wiffSpectrum.NumDataPoints; }
        }

        public Peak1D this[int idx]
        {
            get
            {
                if (idx < 0 || idx >= Length)
                    throw new IndexOutOfRangeException();
                return new Peak1D(
                    wiffSpectrum.GetYValue(idx),
                    wiffSpectrum.GetXValue(idx));
            }
        }

        #endregion

        #region IEnumerable<Peak1D> Members

        private static IEnumerable<Peak1D> Yield(Clearcore2.Data.MassSpectrum wiffSpectrum)
        {
            for (int i = 0; i < wiffSpectrum.NumDataPoints; i++)
                yield return new Peak1D(
                    wiffSpectrum.GetYValue(i),
                    wiffSpectrum.GetXValue(i));
        }

        public IEnumerator<Peak1D> GetEnumerator()
        {
            return Yield(wiffSpectrum).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Yield(wiffSpectrum).GetEnumerator();
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
