#region license
// The MIT License (MIT)

// BafFileReader.cs

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
using System.Threading.Tasks;
using MzLite.Binary;
using MzLite.IO;
using MzLite.Json;
using MzLite.Model;
using System.Linq;
using MzLite.MetaData.PSIMS;
using MzLite.MetaData.UO;
using MzLite.MetaData;
using MzLite.Commons.Arrays;
using System.Collections.ObjectModel;

namespace MzLite.Bruker
{


    public sealed class BafFileReader : IMzLiteDataReader
    {

        private bool disposed = false;
        private readonly string bafFilePath;
        private readonly string sqlFilePath;
        private readonly UInt64 baf2SqlHandle = 0;
        private readonly MzLiteModel model;
        private readonly Linq2BafSql linq2BafSql;
        private readonly SupportedVariablesCollection supportedVariables;

        public BafFileReader(string bafFilePath)
        {
            if (string.IsNullOrWhiteSpace(bafFilePath))
                throw new ArgumentNullException("bafFilePath");
            if (!File.Exists(bafFilePath))
                throw new FileNotFoundException("Baf file not exists.");

            this.bafFilePath = bafFilePath;

            try
            {

                sqlFilePath = Baf2SqlWrapper.GetSQLiteCacheFilename(bafFilePath);

                // First argument = 1, ignore contents of Calibrator.ami (if it exists)
                baf2SqlHandle = Baf2SqlWrapper.baf2sql_array_open_storage(1, bafFilePath);

                if (baf2SqlHandle == 0)
                {
                    Baf2SqlWrapper.ThrowLastBaf2SqlError();
                }

                linq2BafSql = new Linq2BafSql(sqlFilePath);

                model = MzLiteJson.HandleExternalModelFile(this, GetModelFilePath());
                supportedVariables = SupportedVariablesCollection.ReadSupportedVariables(linq2BafSql);
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException(ex.Message, ex);
            }
        }

        #region IMzLiteDataReader Members

        public IEnumerable<MassSpectrum> ReadMassSpectra(string runID)
        {

            RaiseDisposed();

            try
            {
                return YieldMassSpectra();
            }
            catch (MzLiteIOException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException("Error reading spectrum.", ex);
            }
        }

        public MassSpectrum ReadMassSpectrum(string spectrumID)
        {

            RaiseDisposed();

            try
            {
                UInt64 id = UInt64.Parse(spectrumID);
                return ReadMassSpectrum(id);
            }
            catch (MzLiteIOException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException("Error reading spectrum: " + spectrumID, ex);
            }
        }

        public Peak1DArray ReadSpectrumPeaks(string spectrumID)
        {
            RaiseDisposed();

            try
            {
                UInt64 id = UInt64.Parse(spectrumID);
                return ReadSpectrumPeaks(id);
            }
            catch (MzLiteIOException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException("Error reading spectrum peaks: " + spectrumID, ex);
            }
        }

        public Task<MassSpectrum> ReadMassSpectrumAsync(string spectrumID)
        {
            return Task<MassSpectrum>.Run(() => { return ReadMassSpectrum(spectrumID); });
        }

        public Task<Peak1DArray> ReadSpectrumPeaksAsync(string spectrumID)
        {
            return Task<Peak1DArray>.Run(() => { return ReadSpectrumPeaks(spectrumID); });
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
            string modelName = Path.GetFileNameWithoutExtension(bafFilePath);
            MzLiteModel model = new MzLiteModel(modelName);

            string sampleName = Path.GetFileNameWithoutExtension(bafFilePath);
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
            return new BafFileTransactionScope();
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

            if (baf2SqlHandle != 0)
            {
                Baf2SqlWrapper.baf2sql_array_close_storage(baf2SqlHandle);
            }

            if (linq2BafSql != null)
            {
                linq2BafSql.Dispose();
            }

            disposed = true;
        }

        #endregion

        #region BafFileReader Members

        private string GetModelFilePath()
        {
            return bafFilePath + ".mzlitemodel";
        }        

        private MassSpectrum ReadMassSpectrum(UInt64 spectrumId)
        {

            BafSqlSpectrum bafSpec = linq2BafSql.GetBafSqlSpectrum(this.linq2BafSql.Core, spectrumId);

            if (bafSpec == null)
                throw new MzLiteIOException("No spectrum found for id: " + spectrumId);

            MassSpectrum ms = new MassSpectrum(spectrumId.ToString());

            // determine ms level
            BafSqlAcquisitionKey aqKey = linq2BafSql.GetBafSqlAcquisitionKey(this.linq2BafSql.Core, bafSpec.AcquisitionKey);
            Nullable<int> msLevel = null;

            if (aqKey != null && aqKey.MsLevel.HasValue)
            {
                // bruker starts ms level by 0, must be added by 1
                msLevel = aqKey.MsLevel.Value + 1;
                ms.SetMsLevel(msLevel.Value);
            }

            // determine type of spectrum and read peak data
            // if profile data available we prefer to get profile data otherwise centroided data (line spectra)
            if (bafSpec.ProfileMzId.HasValue && bafSpec.ProfileIntensityId.HasValue)
            {
                ms.SetProfileSpectrum();
            }
            else if (bafSpec.LineMzId.HasValue && bafSpec.LineIntensityId.HasValue)
            {
                ms.SetCentroidSpectrum();
            }

            if (msLevel == 1)
            {
                ms.SetMS1Spectrum();
            }
            else if (msLevel > 1)
            {
                ms.SetMSnSpectrum();
            }

            // scan
            if (bafSpec.Rt.HasValue)
            {
                Scan scan = new Scan();
                scan.SetScanStartTime(bafSpec.Rt.Value).UO_Second();
                ms.Scans.Add(scan);
            }

            // precursor
            if (msLevel > 1)
            {

                SpectrumVariableCollection spectrumVariables = SpectrumVariableCollection.ReadSpectrumVariables(linq2BafSql, bafSpec.Id);

                Precursor precursor = new Precursor();

                decimal value;

                if (spectrumVariables.TryGetValue("Collision_Energy_Act", supportedVariables, out value))
                {
                    precursor.Activation.SetCollisionEnergy(Decimal.ToDouble(value));
                }
                if (spectrumVariables.TryGetValue("MSMS_IsolationMass_Act", supportedVariables, out value))
                {
                    precursor.IsolationWindow.SetIsolationWindowTargetMz(Decimal.ToDouble(value));
                }
                if (spectrumVariables.TryGetValue("Quadrupole_IsolationResolution_Act", supportedVariables, out value))
                {
                    double width = Decimal.ToDouble(value) * 0.5d;
                    precursor.IsolationWindow.SetIsolationWindowUpperOffset(width);
                    precursor.IsolationWindow.SetIsolationWindowLowerOffset(width);
                }

                Nullable<int> charge = null;

                if (spectrumVariables.TryGetValue("MSMS_PreCursorChargeState", supportedVariables, out value))
                {
                    charge = Decimal.ToInt32(value);
                }

                IEnumerable<BafSqlStep> ions = linq2BafSql.GetBafSqlSteps(this.linq2BafSql.Core,bafSpec.Id);

                foreach (BafSqlStep ion in ions)
                {
                    if (ion.Mass.HasValue)
                    {
                        SelectedIon selectedIon = new SelectedIon();
                        precursor.SelectedIons.Add(selectedIon);
                        selectedIon.SetSelectedIonMz(ion.Mass.Value);

                        selectedIon.SetUserParam("Number", ion.Number.Value);
                        selectedIon.SetUserParam("IsolationType", ion.IsolationType.Value);
                        selectedIon.SetUserParam("ReactionType", ion.ReactionType.Value);
                        selectedIon.SetUserParam("MsLevel", ion.MsLevel.Value);

                        if (charge.HasValue)
                        {
                            selectedIon.SetChargeState(charge.Value);
                        }
                    }

                }

                // set parent spectrum as reference
                if (bafSpec.Parent.HasValue)
                {
                    precursor.SpectrumReference = new SpectrumReference(bafSpec.Parent.ToString());
                }

                ms.Precursors.Add(precursor);
            }

            return ms;
        }

        private IEnumerable<MassSpectrum> YieldMassSpectra()
        {
            IEnumerable<Nullable<UInt64>> ids = linq2BafSql.Spectra
                .Where(x => x.Id != null)
                .OrderBy(x => x.Rt)
                .Select(x => x.Id)
                .ToArray();

            foreach (var id in ids)
            {
                yield return ReadMassSpectrum(id.Value);
            }
        }

        public Peak1DArray ReadSpectrumPeaks(UInt64 spectrumId)
        {

            BafSqlSpectrum bafSpec = linq2BafSql.GetBafSqlSpectrum(this.linq2BafSql.Core,spectrumId);

            if (bafSpec == null)
                throw new MzLiteIOException("No spectrum found for id: " + spectrumId);

            Peak1DArray pa = new Peak1DArray(
                        BinaryDataCompressionType.NoCompression,
                        BinaryDataType.Float32,
                        BinaryDataType.Float32);

            double[] masses;
            UInt32[] intensities;

            // if profile data available we prefer to get profile data otherwise centroided data (line spectra)
            if (bafSpec.ProfileMzId.HasValue && bafSpec.ProfileIntensityId.HasValue)
            {
                masses = Baf2SqlWrapper.GetBafDoubleArray(baf2SqlHandle, bafSpec.ProfileMzId.Value);
                intensities = Baf2SqlWrapper.GetBafUInt32Array(baf2SqlHandle, bafSpec.ProfileIntensityId.Value);
            }
            else if (bafSpec.LineMzId.HasValue && bafSpec.LineIntensityId.HasValue)
            {
                masses = Baf2SqlWrapper.GetBafDoubleArray(baf2SqlHandle, bafSpec.LineMzId.Value);
                intensities = Baf2SqlWrapper.GetBafUInt32Array(baf2SqlHandle, bafSpec.LineIntensityId.Value);
            }
            else
            {
                masses = new double[0];
                intensities = new UInt32[0];
            }

            pa.Peaks = new BafPeaksArray(masses, intensities);

            return pa;
        }        

        #endregion

        private class BafFileTransactionScope : ITransactionScope
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

        private class BafPeaksArray : IMzLiteArray<Peak1D>
        {

            private readonly double[] masses;
            private readonly UInt32[] intensities;

            public BafPeaksArray(double[] masses, UInt32[] intensities)
            {
                this.masses = masses;
                this.intensities = intensities;
            }

            #region IMzLiteArray<Peak1D> Members

            public int Length
            {
                get { return Math.Min(masses.Length, intensities.Length); }
            }

            public Peak1D this[int idx]
            {
                get
                {
                    if (idx < 0 || idx >= Length)
                        throw new IndexOutOfRangeException();
                    return new Peak1D(
                        intensities[idx],
                        masses[idx]);
                }
            }

            #endregion

            #region IEnumerable<Peak1D> Members

            private IEnumerable<Peak1D> Yield()
            {
                for (int i = 0; i < Length; i++)
                    yield return this[i];
            }

            public IEnumerator<Peak1D> GetEnumerator()
            {
                return Yield().GetEnumerator();
            }

            #endregion

            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return Yield().GetEnumerator();
            }

            #endregion
        }

        private class SupportedVariablesCollection : KeyedCollection<string, BafSqlSupportedVariable>
        {

            private SupportedVariablesCollection()
            {
            }

            public static SupportedVariablesCollection ReadSupportedVariables(Linq2BafSql linq2BafSql)
            {
                var variables = linq2BafSql
                    .SupportedVariables
                    .ToArray()
                    .Where(x => x.Variable.HasValue && string.IsNullOrWhiteSpace(x.PermanentName) == false);

                var col = new SupportedVariablesCollection();

                foreach (var item in variables)
                    col.Add(item);

                return col;
            }

            public bool TryGetItem(string variablePermanentName, out BafSqlSupportedVariable variable)
            {
                if (Contains(variablePermanentName))
                {
                    variable = this[variablePermanentName];
                    return true;
                }
                else
                {
                    variable = null;
                    return false;
                }
            }

            protected override string GetKeyForItem(BafSqlSupportedVariable item)
            {
                return item.PermanentName;
            }
        }

        private class SpectrumVariableCollection : KeyedCollection<ulong, BafSqlPerSpectrumVariable>
        {

            private SpectrumVariableCollection()
            {
            }

            public static SpectrumVariableCollection ReadSpectrumVariables(Linq2BafSql linq2BafSql, UInt64? spectrumId)
            {
                IEnumerable<BafSqlPerSpectrumVariable> variables = linq2BafSql.GetPerSpectrumVariables(linq2BafSql.Core, spectrumId);
                var col = new SpectrumVariableCollection();
                foreach (var v in variables)
                    col.Add(v);

                return col;
            }

            protected override ulong GetKeyForItem(BafSqlPerSpectrumVariable item)
            {
                return item.Variable.Value;
            }

            public bool TryGetValue(string variablePermanentName, SupportedVariablesCollection supportedVariables, out decimal value)
            {
                BafSqlSupportedVariable variable;

                if (supportedVariables.TryGetItem(variablePermanentName, out variable))
                {
                    if (Contains(variable.Variable.Value))
                    {
                        value = this[variable.Variable.Value].Value.Value;
                        return true;
                    }
                    else
                    {
                        value = default(decimal);
                        return false;
                    }
                }
                else
                {
                    value = default(decimal);
                    return false;
                }
            }
        }
    }


}
