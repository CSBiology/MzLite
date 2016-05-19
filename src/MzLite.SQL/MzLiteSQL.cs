using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using MzLite.Binary;
using MzLite.IO;
using MzLite.Json;
using MzLite.Model;

namespace MzLite.SQL
{
    public class MzLiteSQL : IDisposable
    {

        private readonly BinaryDataEncoder encoder = new BinaryDataEncoder();
        private readonly SQLiteConnection connection;
        private bool disposed = false;
        private MzLiteSQLTransactionScope currentScope = null;

        public MzLiteSQL(string path)
        {

            if (path == null)
                throw new ArgumentNullException("path");

            try
            {                
                if (!File.Exists(path))
                {
                    connection = CreateSchema(path);
                }
                else
                {
                    connection = OpenSchema(path);
                }
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException("Error opening mzlite sql file.", ex);
            }
        }

        public bool IsOpenScope { get { return currentScope != null; } }

        public MzLiteSQLTransactionScope BeginTransaction()
        {
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

        public void Insert(MassSpectrum spectrum, Peak1DArray peaks)
        {
            if (IsOpenScope)
            {
                InsertCmd(spectrum, peaks);
            }
            else
            {
                using (var scope = BeginTransaction())
                {
                    InsertCmd(spectrum, peaks);
                    scope.Commit();
                }
            }
        }        

        #region MzSQLWriter Members

        private static SQLiteConnection GetConnection(string path)
        {
            SQLiteConnection conn =
                new SQLiteConnection(string.Format("DataSource={0}", path));
            if (conn.State != ConnectionState.Open)
                conn.Open();
            return conn;
        }

        private static void RunPragmas(SQLiteConnection conn)
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

        private static SQLiteConnection CreateSchema(string path)
        {

            using (File.Create(path)) { }

            SQLiteConnection conn = GetConnection(path);
            RunPragmas(conn);

            using (var txn = conn.BeginTransaction())
            {
                try
                {
                    var cmd = conn.CreateCommand();
                    cmd.Transaction = txn;
                    cmd.CommandText = "CREATE TABLE Model (Lock INTEGER PRIMARY KEY DEFAULT(0) CHECK (Lock=0), Content TEXT NOT NULL)";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "CREATE TABLE Spectrum (RunID TEXT, SpectrumID TEXT, Description TEXT NOT NULL, PeakArray TEXT NOT NULL, PeakData BINARY NOT NULL, PRIMARY KEY (RunID, SpectrumID));";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "CREATE TABLE Chromatogram (RunID TEXT, ChromatogramID TEXT, Description TEXT NOT NULL, PeakArray TEXT NOT NULL, PeakData BINARY NOT NULL, PRIMARY KEY (RunID, ChromatogramID));";
                    cmd.ExecuteNonQuery();
                    txn.Commit();
                }
                catch
                {
                    txn.Rollback();
                    throw;
                }
            }

            return conn;
        }

        private static SQLiteConnection OpenSchema(string path)
        {
            SQLiteConnection conn = GetConnection(path);
            RunPragmas(conn);
            return conn;
        }

        internal void ReleaseTransactionScope()
        {            
            currentScope = null;
        }

        private void InsertCmd(MassSpectrum spectrum, Peak1DArray peaks)
        {
            try
            {
                SQLiteCommand cmd;

                if (!currentScope.TryGetCommand("INSERT_SPECTRUM_CMD", out cmd))
                {
                    cmd = currentScope.PrepareCommand("INSERT_SPECTRUM_CMD", "INSERT INTO Spectrum VALUES(@runID, @spectrumID, @description, @peakArray, @peakData);");
                }

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@runID", spectrum.ID.RunID);
                cmd.Parameters.AddWithValue("@spectrumID", spectrum.ID.SpectrumID);
                cmd.Parameters.AddWithValue("@description", MzLiteJson.ToJson(spectrum));
                cmd.Parameters.AddWithValue("@peakArray", MzLiteJson.ToJson(peaks));
                cmd.Parameters.AddWithValue("@peakData", encoder.Encode(peaks));

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new MzLiteIOException("Error execute insert spectrum command.", ex);
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
    public class MzLiteSQLTransactionScope : IDisposable
    {

        private readonly SQLiteConnection connection;
        private readonly SQLiteTransaction transaction;
        private readonly MzLiteSQL writer;
        private readonly IDictionary<string, SQLiteCommand> commands = new Dictionary<string, SQLiteCommand>();

        private bool disposed = false;

        internal MzLiteSQLTransactionScope(MzLiteSQL writer, SQLiteConnection connection)
        {
            this.connection = connection;
            this.transaction = connection.BeginTransaction();
            this.writer = writer;
        }

        public SQLiteCommand PrepareCommand(string name, string commandText)
        {
            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = commandText;
            cmd.Transaction = transaction;
            cmd.Prepare();
            commands[name] = cmd;
            return cmd;
        }

        public bool TryGetCommand(string name, out SQLiteCommand cmd)
        {
            return commands.TryGetValue(name, out cmd);
        }

        public void Commit()
        {
            RaiseDisposed();
            transaction.Commit();
        }

        public void Rollback()
        {
            RaiseDisposed();
            transaction.Rollback();
        }

        public void Close()
        {
            foreach (var cmd in commands.Values)
                cmd.Dispose();
            commands.Clear();
            if (transaction != null)
                transaction.Dispose();
            writer.ReleaseTransactionScope();
        }

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
            Close();
            disposed = true;
        }

        #endregion
    }
}
