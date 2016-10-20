#region license
// The MIT License (MIT)

// MzLiteSQL.cs

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
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using MzLite.Binary;
using MzLite.IO;
using MzLite.Json;
using MzLite.Model;

namespace MzLite.SQL
{

    /// <summary>
    /// The MzLite data reader/writer implementation for SQLite databases.
    /// </summary>
    public class MzLiteSQL : IMzLiteDataWriter, IMzLiteDataReader
    {

        private readonly BinaryDataEncoder encoder = new BinaryDataEncoder();
        private readonly BinaryDataDecoder decoder = new BinaryDataDecoder();
        private readonly SQLiteConnection connection;
        private readonly MzLiteModel model;
        private bool disposed = false;
        private MzLiteSQLTransactionScope currentScope = null;

        public MzLiteSQL(string path)
        {

            if (path == null)
                throw new ArgumentNullException("path");

            try
            {
                if (!File.Exists(path))
                    using (File.Create(path)) { }

                connection = GetConnection(path);
                SqlRunPragmas(connection);

                using (var scope = BeginTransaction())
                {

                    try
                    {

                        SqlInitSchema();

                        if (!SqlTrySelect(out model))
                        {
                            model = new MzLiteModel(Path.GetFileNameWithoutExtension(path));
                            SqlSave(model);
                        }

                        scope.Commit();
                    }
                    catch
                    {
                        scope.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException("Error opening mzlite sql file.", ex);
            }
        }

        #region IMzLiteIO Members

        public ITransactionScope BeginTransaction()
        {

            RaiseDisposed();

            if (IsOpenScope)
                throw new MzLiteIOException("Illegal attempt transaction scope reentrancy.");

            try
            {
                currentScope = new MzLiteSQLTransactionScope(this, connection);
                return currentScope;
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException(ex.Message, ex);
            }
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
            RaiseNotInScope();

            try
            {
                SqlSave(model);
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException(ex.Message, ex);
            }
        }

        internal void ReleaseTransactionScope()
        {
            currentScope = null;
        }

        private bool IsOpenScope { get { return currentScope != null; } }

        private void RaiseNotInScope()
        {
            if (!IsOpenScope)
                throw new MzLiteIOException("No transaction scope was initialized.");
        }

        #endregion

        #region IMzLiteDataWriter Members

        public void Insert(string runID, MassSpectrum spectrum, Peak1DArray peaks)
        {
            RaiseDisposed();
            RaiseNotInScope();

            try
            {
                SqlInsert(runID, spectrum, peaks);
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException(ex.Message, ex);
            }

        }

        public void Insert(string runID, Chromatogram chromatogram, Peak2DArray peaks)
        {

            RaiseDisposed();
            RaiseNotInScope();

            try
            {
                SqlInsert(runID, chromatogram, peaks);
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException(ex.Message, ex);
            }

        }

        public Task InsertAsync(string runID, MassSpectrum spectrum, Peak1DArray peaks)
        {
            return Task.Run(() => { Insert(runID, spectrum, peaks); });
        }

        public Task InsertAsync(string runID, Chromatogram chromatogram, Peak2DArray peaks)
        {
            return Task.Run(() => { Insert(runID, chromatogram, peaks); });
        }

        #endregion

        #region IMzLiteDataReader Members

        public IEnumerable<MassSpectrum> ReadMassSpectra(string runID)
        {

            RaiseDisposed();
            RaiseNotInScope();

            try
            {
                return SqlSelectMassSpectra(runID);
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException(ex.Message, ex);
            }
        }

        public MassSpectrum ReadMassSpectrum(string spectrumID)
        {
            RaiseDisposed();
            RaiseNotInScope();

            try
            {
                MassSpectrum ms;
                if (SqlTrySelect(spectrumID, out ms))
                    return ms;
                else
                    throw new MzLiteIOException(string.Format("Spectrum for id '{0}' not found.", spectrumID));
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException(ex.Message, ex);
            }

        }

        public Peak1DArray ReadSpectrumPeaks(string spectrumID)
        {
            RaiseDisposed();
            RaiseNotInScope();

            try
            {
                Peak1DArray peaks;
                if (SqlTrySelect(spectrumID, out peaks))
                    return peaks;
                else
                    throw new MzLiteIOException(string.Format("Spectrum with id '{0}' not found.", spectrumID));
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

            RaiseDisposed();
            RaiseNotInScope();

            try
            {
                return SqlSelectChromatograms(runID);
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException(ex.Message, ex);
            }
        }

        public Chromatogram ReadChromatogram(string chromatogramID)
        {
            RaiseDisposed();
            RaiseNotInScope();

            try
            {
                Chromatogram ch;
                if (SqlTrySelect(chromatogramID, out ch))
                    return ch;
                else
                    throw new MzLiteIOException(string.Format("Chromatogram for id '{0}' not found.", chromatogramID));
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException(ex.Message, ex);
            }
        }

        public Peak2DArray ReadChromatogramPeaks(string chromatogramID)
        {
            RaiseDisposed();
            RaiseNotInScope();

            try
            {
                Peak2DArray peaks;
                if (SqlTrySelect(chromatogramID, out peaks))
                    return peaks;
                else
                    throw new MzLiteIOException(string.Format("Chromatogram for id '{0}' not found.", chromatogramID));
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException(ex.Message, ex);
            }

        }

        public Task<Chromatogram> ReadChromatogramAsync(string spectrumID)
        {
            return Task<Chromatogram>.Run(() => { return ReadChromatogram(spectrumID); });
        }

        public Task<Peak2DArray> ReadChromatogramPeaksAsync(string spectrumID)
        {
            return Task<Peak2DArray>.Run(() => { return ReadChromatogramPeaks(spectrumID); });
        }

        #endregion

        #region sql statements

        private static SQLiteConnection GetConnection(string path)
        {
            SQLiteConnection conn =
                new SQLiteConnection(string.Format("DataSource={0}", path));
            if (conn.State != ConnectionState.Open)
                conn.Open();
            return conn;
        }

        private static void SqlRunPragmas(SQLiteConnection conn)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "PRAGMA synchronous=OFF";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "PRAGMA journal_mode=MEMORY";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "PRAGMA temp_store=MEMORY";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "PRAGMA ignore_check_constraints=OFF";
                cmd.ExecuteNonQuery();
            }
        }

        private void SqlInitSchema()
        {
            using (SQLiteCommand cmd = currentScope.CreateCommand("CREATE TABLE IF NOT EXISTS Model (Lock INTEGER  NOT NULL PRIMARY KEY DEFAULT(0) CHECK (Lock=0), Content TEXT NOT NULL)"))
                cmd.ExecuteNonQuery();
            using (SQLiteCommand cmd = currentScope.CreateCommand("CREATE TABLE IF NOT EXISTS Spectrum (RunID TEXT NOT NULL, SpectrumID TEXT NOT NULL PRIMARY KEY, Description TEXT NOT NULL, PeakArray TEXT NOT NULL, PeakData BINARY NOT NULL);"))
                cmd.ExecuteNonQuery();
            using (SQLiteCommand cmd = currentScope.CreateCommand("CREATE TABLE IF NOT EXISTS Chromatogram (RunID TEXT NOT NULL, ChromatogramID TEXT NOT NULL PRIMARY KEY, Description TEXT NOT NULL, PeakArray TEXT NOT NULL, PeakData BINARY NOT NULL);"))
                cmd.ExecuteNonQuery();
        }

        private void SqlInsert(string runID, MassSpectrum spectrum, Peak1DArray peaks)
        {
            SQLiteCommand cmd;

            if (!currentScope.TryGetCommand("INSERT_SPECTRUM_CMD", out cmd))
            {
                cmd = currentScope.PrepareCommand("INSERT_SPECTRUM_CMD", "INSERT INTO Spectrum VALUES(@runID, @spectrumID, @description, @peakArray, @peakData);");
            }

            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@runID", runID);
            cmd.Parameters.AddWithValue("@spectrumID", spectrum.ID);
            cmd.Parameters.AddWithValue("@description", MzLiteJson.ToJson(spectrum));
            cmd.Parameters.AddWithValue("@peakArray", MzLiteJson.ToJson(peaks));
            cmd.Parameters.AddWithValue("@peakData", encoder.Encode(peaks));

            cmd.ExecuteNonQuery();

        }

        private void SqlInsert(string runID, Chromatogram chromatogram, Peak2DArray peaks)
        {

            SQLiteCommand cmd;

            if (!currentScope.TryGetCommand("INSERT_CHROMATOGRAM_CMD", out cmd))
            {
                cmd = currentScope.PrepareCommand("INSERT_CHROMATOGRAM_CMD", "INSERT INTO Chromatogram VALUES(@runID, @chromatogramID, @description, @peakArray, @peakData);");
            }

            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@runID", runID);
            cmd.Parameters.AddWithValue("@chromatogramID", chromatogram.ID);
            cmd.Parameters.AddWithValue("@description", MzLiteJson.ToJson(chromatogram));
            cmd.Parameters.AddWithValue("@peakArray", MzLiteJson.ToJson(peaks));
            cmd.Parameters.AddWithValue("@peakData", encoder.Encode(peaks));

            cmd.ExecuteNonQuery();

        }

        private void SqlSave(MzLiteModel model)
        {
            using (SQLiteCommand cmd = currentScope.CreateCommand("DELETE FROM Model"))
            {
                cmd.ExecuteNonQuery();
            }

            using (SQLiteCommand cmd = currentScope.CreateCommand("INSERT INTO Model VALUES(@lock, @content)"))
            {
                cmd.Parameters.AddWithValue("@lock", 0);
                cmd.Parameters.AddWithValue("@content", MzLiteJson.ToJson(model));
                cmd.ExecuteNonQuery();
            }

        }

        private bool SqlTrySelect(out MzLiteModel model)
        {
            using (SQLiteCommand cmd = currentScope.CreateCommand("SELECT Content FROM Model"))
            {
                string content = cmd.ExecuteScalar() as string;

                if (content != null)
                {
                    model = MzLiteJson.FromJson<MzLiteModel>(content);
                    return true;
                }
                else
                {
                    model = null;
                    return false;
                }
            }

        }

        private IEnumerable<MassSpectrum> SqlSelectMassSpectra(string runID)
        {

            using (SQLiteCommand cmd = currentScope.CreateCommand("SELECT Description FROM Spectrum WHERE RunID = @runID"))
            {

                cmd.Parameters.AddWithValue("@runID", runID);

                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return MzLiteJson.FromJson<MassSpectrum>(reader.GetString(0));
                    }
                }
            }

        }

        private bool SqlTrySelect(string spectrumID, out MassSpectrum ms)
        {

            SQLiteCommand cmd;

            if (!currentScope.TryGetCommand("SELECT_SPECTRUM_CMD", out cmd))
            {
                cmd = currentScope.PrepareCommand("SELECT_SPECTRUM_CMD", "SELECT Description FROM Spectrum WHERE SpectrumID = @spectrumID");
            }
            else
            {
                cmd.Parameters.Clear();
            }

            cmd.Parameters.AddWithValue("@spectrumID", spectrumID);

            string desc = cmd.ExecuteScalar() as string;

            if (desc != null)
            {
                ms = MzLiteJson.FromJson<MassSpectrum>(desc);
                return true;
            }
            else
            {
                ms = null;
                return false;
            }

        }

        private bool SqlTrySelect(string spectrumID, out Peak1DArray peaks)
        {

            SQLiteCommand cmd;

            if (!currentScope.TryGetCommand("SELECT_SPECTRUM_PEAKS_CMD", out cmd))
            {
                cmd = currentScope.PrepareCommand("SELECT_SPECTRUM_PEAKS_CMD", "SELECT PeakArray, PeakData FROM Spectrum WHERE SpectrumID = @spectrumID");
            }
            else
            {
                cmd.Parameters.Clear();
            }

            cmd.Parameters.AddWithValue("@spectrumID", spectrumID);

            using (SQLiteDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    peaks = MzLiteJson.FromJson<Peak1DArray>(reader.GetString(0));
                    decoder.Decode(reader.GetStream(1), peaks);
                    return true;
                }
                else
                {
                    peaks = null;
                    return false;
                }
            }

        }

        private IEnumerable<Chromatogram> SqlSelectChromatograms(string runID)
        {

            using (SQLiteCommand cmd = currentScope.CreateCommand("SELECT Description FROM Chromatogram WHERE RunID = @runID"))
            {

                cmd.Parameters.AddWithValue("@runID", runID);

                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return MzLiteJson.FromJson<Chromatogram>(reader.GetString(0));
                    }
                }
            }

        }

        private bool SqlTrySelect(string chromatogramID, out Chromatogram chromatogram)
        {

            SQLiteCommand cmd;

            if (!currentScope.TryGetCommand("SELECT_CHROMATOGRAM_CMD", out cmd))
            {
                cmd = currentScope.PrepareCommand("SELECT_CHROMATOGRAM_CMD", "SELECT Description FROM Chromatogram WHERE ChromatogramID = @chromatogramID");
            }
            else
            {
                cmd.Parameters.Clear();
            }

            cmd.Parameters.AddWithValue("@chromatogramID", chromatogramID);

            string desc = cmd.ExecuteScalar() as string;

            if (desc != null)
            {
                chromatogram = MzLiteJson.FromJson<Chromatogram>(desc);
                return true;
            }
            else
            {
                chromatogram = null;
                return false;
            }

        }

        private bool SqlTrySelect(string chromatogramID, out Peak2DArray peaks)
        {

            SQLiteCommand cmd;

            if (!currentScope.TryGetCommand("SELECT_CHROMATOGRAM_PEAKS_CMD", out cmd))
            {
                cmd = currentScope.PrepareCommand("SELECT_CHROMATOGRAM_PEAKS_CMD", "SELECT PeakArray, PeakData FROM Chromatogram WHERE ChromatogramID = @chromatogramID");
            }
            else
            {
                cmd.Parameters.Clear();
            }

            cmd.Parameters.AddWithValue("@chromatogramID", chromatogramID);

            using (SQLiteDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    peaks = MzLiteJson.FromJson<Peak2DArray>(reader.GetString(0));
                    decoder.Decode(reader.GetStream(1), peaks);
                    return true;
                }
                else
                {
                    peaks = null;
                    return false;
                }
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
            if (currentScope != null)
                currentScope.Dispose();
            if (connection != null)
                connection.Dispose();
            disposed = true;
        }

        #endregion
    }

    /// <summary>
    /// Provides prepared statements within a SQLite connection.
    /// </summary>
    internal class MzLiteSQLTransactionScope : ITransactionScope
    {

        private readonly SQLiteConnection connection;
        private readonly SQLiteTransaction transaction;
        private readonly MzLiteSQL writer;
        private readonly IDictionary<string, SQLiteCommand> commands = new Dictionary<string, SQLiteCommand>();
        private bool disposed = false;

        #region ITransactionScope Members

        public void Commit()
        {
            RaiseDisposed();

            try
            {
                transaction.Commit();
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException(ex.Message, ex);
            }
        }

        public void Rollback()
        {
            RaiseDisposed();

            try
            {
                transaction.Rollback();
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException(ex.Message, ex);
            }
        }

        #endregion

        #region MzLiteSQLTransactionScope Members

        internal MzLiteSQLTransactionScope(MzLiteSQL writer, SQLiteConnection connection)
        {
            this.connection = connection;
            this.transaction = connection.BeginTransaction();
            this.writer = writer;
        }

        internal SQLiteCommand PrepareCommand(string name, string commandText)
        {

            RaiseDisposed();

            SQLiteCommand cmd = CreateCommand(commandText);
            cmd.Prepare();
            commands[name] = cmd;
            return cmd;
        }

        internal SQLiteCommand CreateCommand(string commandText)
        {

            RaiseDisposed();

            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = commandText;
            cmd.Transaction = transaction;
            return cmd;
        }

        internal bool TryGetCommand(string name, out SQLiteCommand cmd)
        {
            RaiseDisposed();
            return commands.TryGetValue(name, out cmd);
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

            foreach (var cmd in commands.Values)
                cmd.Dispose();
            commands.Clear();

            if (transaction != null)
                transaction.Dispose();

            writer.ReleaseTransactionScope();

            disposed = true;
        }

        #endregion
    }
}
