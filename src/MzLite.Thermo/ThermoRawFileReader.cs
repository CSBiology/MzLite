#region license
// The MIT License (MIT)

// ThermoRawFileReader.cs

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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MSFileReaderLib;
using MzLite.Binary;
using MzLite.Commons.Arrays;
using MzLite.IO;
using MzLite.Json;
using MzLite.MetaData.PSIMS;
using MzLite.Model;

namespace MzLite.Thermo
{
    public sealed class ThermoRawFileReader : IMzLiteDataReader
    {

        private bool disposed = false;
        private readonly MzLiteModel model;
        private readonly string rawFilePath;
        private readonly IXRawfile5 rawFile;
        private readonly int startScanNo;
        private readonly int endScanNo;

        public ThermoRawFileReader(string rawFilePath)
        {
            this.rawFilePath = rawFilePath;

            if (string.IsNullOrWhiteSpace(rawFilePath))
                throw new ArgumentNullException("rawFilePath");
            if (!File.Exists(rawFilePath))
                throw new FileNotFoundException("Raw file not exists.");

            try
            {

                rawFile = new MSFileReader_XRawfile() as IXRawfile5;
                rawFile.Open(rawFilePath);
                rawFile.SetCurrentController(0, 1);

                startScanNo = GetFirstSpectrumNumber(rawFile);
                endScanNo = GetLastSpectrumNumber(rawFile);

                model = MzLiteJson.HandleExternalModelFile(this, GetModelFilePath());
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException(ex.Message, ex);
            }
        }

        #region ThermoRawFileReader Members

        private string GetModelFilePath()
        {
            return rawFilePath + ".mzlitemodel";
        }

        private static int GetFirstSpectrumNumber(IXRawfile5 rawFile)
        {
            int firstScanNumber = 0;
            rawFile.GetFirstSpectrumNumber(ref firstScanNumber);
            return firstScanNumber;
        }

        private static int GetLastSpectrumNumber(IXRawfile5 rawFile)
        {
            int lastScanNumber = 0;
            rawFile.GetLastSpectrumNumber(ref lastScanNumber);
            return lastScanNumber;
        }

        private static bool IsCentroidSpectrum(IXRawfile5 rawFile, int scanNo)
        {
            int isCentroidScan = 0;
            rawFile.IsCentroidScanForScanNum(scanNo, ref isCentroidScan);
            return isCentroidScan != 0;
        }

        private static double GetRetentionTime(IXRawfile5 rawFile, int scanNo)
        {
            double rt = 0;
            rawFile.RTFromScanNum(scanNo, ref rt);
            return rt;
        }

        private static string GetFilterString(IXRawfile5 rawFile, int scanNo)
        {
            string filter = null;
            rawFile.GetFilterForScanNum(scanNo, ref filter);
            return filter;
        }

        private static int GetMSLevel(IXRawfile5 rawFile, int scanNo)
        {
            int msLevel = 1;
            rawFile.GetMSOrderForScanNum(scanNo, ref msLevel);
            return msLevel;
        }

        private static double GetIsolationWindowWidth(IXRawfile5 rawFile, int scanNo, int msLevel)
        {
            double width = 0;
            rawFile.GetIsolationWidthForScanNum(scanNo, msLevel - 1, ref width);
            return width;
        }

        private static double GetIsolationWindowTargetMz(IXRawfile5 rawFile, int scanNo, int msLevel)
        {
            double mz = 0;
            rawFile.GetPrecursorMassForScanNum(scanNo, msLevel, ref mz);
            return mz;
        }

        private static double GetPrecursorMz(IXRawfile5 rawFile, int scanNo, int msLevel)
        {
            double mz = 0;
            rawFile.GetPrecursorMassForScanNum(scanNo, msLevel, ref mz);
            return mz;
        }

        private static double GetCollisionEnergy(IXRawfile5 rawFile, int scanNo, int msLevel)
        {
            double e = 0;
            rawFile.GetCollisionEnergyForScanNum(scanNo, msLevel - 1, ref e);
            return e;
        }

        private static int GetChargeState(IXRawfile5 rawFile, int scanNo)
        {
            object pChargeValue = null;
            rawFile.GetTrailerExtraValueForScanNum(scanNo, "Charge State:", ref pChargeValue);
            return Convert.ToInt32(pChargeValue);
        }

        /// <summary>
        /// Parse the scan number from spectrum native id.
        /// Native id has the format: 'scan=[scanNumber]', i.e. for scan number 1000 the id is 'scan=1000'.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private int ParseSpectrumId(string id)
        {
            if (id == null)
                throw new ArgumentNullException("id");

            string[] splitted = id.Split('=');

            if (splitted.Length == 2 && splitted[0].Equals("scan"))
            {
                int scanNo = Int32.Parse(splitted[1]);
                if (scanNo < startScanNo || scanNo > endScanNo)
                    throw new IndexOutOfRangeException("Scan number out of range.");
                return scanNo;
            }
            else
            {
                throw new FormatException("Wrong native id format in: " + id);
            }
        }

        /// <summary>
        /// Builds a spectrum id from scan number in format : 'scan=scanNumber'.
        /// </summary>
        /// <param name="scanNumber"></param>
        /// <returns></returns>
        private static string GetSpectrumID(int scanNumber)
        {
            return "scan=" + scanNumber.ToString();
        }

        private MzLite.Model.MassSpectrum ReadMassSpectrum(int scanNo)
        {

            RaiseDisposed();

            try
            {

                string spectrumID = GetSpectrumID(scanNo);
                MassSpectrum spectrum = new MassSpectrum(spectrumID);

                // spectrum

                int msLevel = GetMSLevel(rawFile, scanNo);
                spectrum.SetMsLevel(msLevel);

                if (IsCentroidSpectrum(rawFile, scanNo))
                    spectrum.SetCentroidSpectrum();
                else
                    spectrum.SetProfileSpectrum();

                // scan

                Scan scan = new Scan();
                scan.SetFilterString(GetFilterString(rawFile, scanNo))
                    .SetScanStartTime(GetRetentionTime(rawFile, scanNo));
                //.UO_Minute();

                spectrum.Scans.Add(scan);

                // precursor

                if (msLevel > 1)
                {

                    Precursor precursor = new Precursor();

                    double isoWidth = GetIsolationWindowWidth(rawFile, scanNo, msLevel) * 0.5d;
                    double targetMz = GetIsolationWindowTargetMz(rawFile, scanNo, msLevel);
                    double precursorMz = GetPrecursorMz(rawFile, scanNo, msLevel);
                    int chargeState = GetChargeState(rawFile, scanNo);

                    precursor.IsolationWindow
                            .SetIsolationWindowTargetMz(targetMz)
                            .SetIsolationWindowUpperOffset(isoWidth)
                            .SetIsolationWindowLowerOffset(isoWidth);

                    SelectedIon selectedIon = new SelectedIon();

                    selectedIon
                        .SetSelectedIonMz(precursorMz)
                        .SetChargeState(chargeState);

                    precursor.SelectedIons.Add(selectedIon);

                    spectrum.Precursors.Add(precursor);
                }

                return spectrum;

            }
            catch (Exception ex)
            {
                throw new MzLiteIOException(ex.Message, ex);
            }
        }

        private Binary.Peak1DArray ReadSpectrumPeaks(int scanNo)
        {

            RaiseDisposed();

            try
            {
                int peakArraySize = 0;
                double controidPeakWith = 0;
                object massList = null;
                object peakFlags = null;

                rawFile.GetMassListFromScanNum(
                    ref scanNo,
                    null, 1, 0, 0, 0,
                    ref controidPeakWith,
                    ref massList,
                    ref peakFlags,
                    ref peakArraySize);

                double[,] peakData = massList as double[,];

                Peak1DArray pa = new Peak1DArray(
                        BinaryDataCompressionType.NoCompression,
                        BinaryDataType.Float32,
                        BinaryDataType.Float32);

                //Peak1D[] peaks = new Peak1D[peakArraySize];

                //for (int i = 0; i < peakArraySize; i++)
                //    peaks[i] = new Peak1D(peakData[1, i], peakData[0, i]);

                //pa.Peaks = MzLiteArray.ToMzLiteArray(peaks);

                pa.Peaks = new ThermoPeaksArray(peakData, peakArraySize);

                return pa;
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException(ex.Message, ex);
            }
        }

        #endregion

        #region IMzLiteDataReader Members

        public IEnumerable<Model.MassSpectrum> ReadMassSpectra(string runID)
        {
            for (int i = startScanNo; i <= endScanNo; i++)
            {
                yield return ReadMassSpectrum(i);
            }
        }

        public Model.MassSpectrum ReadMassSpectrum(string spectrumID)
        {
            int scanNo = ParseSpectrumId(spectrumID);
            return ReadMassSpectrum(scanNo);
        }

        public Binary.Peak1DArray ReadSpectrumPeaks(string spectrumID)
        {
            int scanNo = ParseSpectrumId(spectrumID);
            return ReadSpectrumPeaks(scanNo);
        }

        public Task<MassSpectrum> ReadMassSpectrumAsync(string spectrumID)
        {
            return Task<MassSpectrum>.Run(() => { return ReadMassSpectrum(spectrumID); });
        }

        public Task<Peak1DArray> ReadSpectrumPeaksAsync(string spectrumID)
        {
            return Task<Peak1DArray>.Run(() => { return ReadSpectrumPeaks(spectrumID); });
        }

        public IEnumerable<Model.Chromatogram> ReadChromatograms(string runID)
        {
            return Enumerable.Empty<Chromatogram>();
        }

        public Model.Chromatogram ReadChromatogram(string chromatogramID)
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

        public Binary.Peak2DArray ReadChromatogramPeaks(string chromatogramID)
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

            string modelName = Path.GetFileNameWithoutExtension(rawFilePath);
            MzLiteModel model = new MzLiteModel(modelName);

            string sampleName = Path.GetFileNameWithoutExtension(rawFilePath);
            Sample sample = new Sample("sample_1", sampleName);
            model.Samples.Add(sample);

            Run run = new Run("run_1");
            run.Sample = sample;
            model.Runs.Add(run);

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
            return new ThermoRawTransactionScope();
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

            if (rawFile != null)
            {
                rawFile.Close();
            }

            disposed = true;
        }

        #endregion
    }

    internal sealed class ThermoPeaksArray : IMzLiteArray<Peak1D>
    {

        private readonly double[,] peakData;
        private readonly int peakArraySize;

        internal ThermoPeaksArray(double[,] peakData, int peakArraySize)
        {
            this.peakData = peakData;
            this.peakArraySize = peakArraySize;
        }

        #region IMzLiteArray<Peak1D> Members

        public int Length
        {
            get { return peakArraySize; }
        }

        public Peak1D this[int idx]
        {
            get { return new Peak1D(peakData[1, idx], peakData[0, idx]); }
        }

        #endregion

        #region IEnumerable<Peak1D> Members

        private static IEnumerable<Peak1D> Yield(double[,] peakData, int peakArraySize)
        {
            for (int i = 0; i < peakArraySize; i++)
            {
                yield return new Peak1D(peakData[1, i], peakData[0, i]);
            }
        }

        public IEnumerator<Peak1D> GetEnumerator()
        {
            return Yield(peakData, peakArraySize).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Yield(peakData, peakArraySize).GetEnumerator();
        }

        #endregion
    }

    internal class ThermoRawTransactionScope : ITransactionScope
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
