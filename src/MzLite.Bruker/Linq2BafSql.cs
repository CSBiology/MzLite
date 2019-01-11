#region license
// The MIT License (MIT)

// Linq2BafSql.cs

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
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.SQLite;
using System.Linq;

namespace MzLite.Bruker
{
    internal sealed class Linq2BafSql :  IDisposable
    {
        
        private static readonly AttributeMappingSource mapping = new AttributeMappingSource();
        private readonly DataContext core;
        private bool isDisposed = false;

        public Linq2BafSql(string sqlFilePath)
        {
            SQLiteConnection cn = new SQLiteConnection("Data Source=" + sqlFilePath + ";Version=3");
            core = new DataContext(cn, mapping)
            {
                DeferredLoadingEnabled = false,
                ObjectTrackingEnabled = false
            };
            // opening a connection at this point leads to a 7 fold speed increase when looking up mass spectra.
            core.Connection.Open();

            // increases speed of massspectra look up but is not compatible with a
            // function in baf2sql_c.dll (probably when creating the sqlite cache) because they are blocking the db access of each another
            // TODO: Examine which baf2sql_c method is causing this.    
            //System.Data.Common.DbTransaction tn = core.Connection.BeginTransaction();
            try
            {
            core.ExecuteQuery<int>("CREATE INDEX StepsID ON Steps (TargetSpectrum)");
            }
            catch
            {
                Console.Out.WriteLine("INDEX On TargedSpectrum in Steps table already exists, creation is skipped");
            }
            try
            {
                core.ExecuteQuery<int>("CREATE INDEX SpectrumID ON PerSpectrumVariables (Spectrum)");
            }
            catch
            {
                Console.Out.WriteLine("INDEX On SpectrumID in PerSpectrumVariables table already exists, creation is skipped");
            }
        }


        public DataContext Core { get { return core; } }

        public IQueryable<BafSqlSpectrum> Spectra { get { return core.GetTable<BafSqlSpectrum>(); } }

        public IQueryable<BafSqlAcquisitionKey> AcquisitionKeys { get { return core.GetTable<BafSqlAcquisitionKey>(); } }

        public IQueryable<BafSqlPerSpectrumVariable> PerSpectrumVariables { get { return core.GetTable<BafSqlPerSpectrumVariable>(); } }

        public IQueryable<BafSqlSupportedVariable> SupportedVariables { get { return core.GetTable<BafSqlSupportedVariable>(); } }

        public IQueryable<BafSqlStep> Steps { get { return core.GetTable<BafSqlStep>(); } }

        public Func<DataContext, UInt64, BafSqlSpectrum> GetBafSqlSpectrum =
                CompiledQuery.Compile((DataContext db, UInt64 id) => db.GetTable<BafSqlSpectrum>().Where(x => x.Id == id).SingleOrDefault());

        public Func<DataContext, UInt64?, BafSqlAcquisitionKey> GetBafSqlAcquisitionKey =
                CompiledQuery.Compile((DataContext db, UInt64? id) => db.GetTable<BafSqlAcquisitionKey>().Where(x => x.Id == id).SingleOrDefault());

        public Func<DataContext, UInt64?, IEnumerable<BafSqlStep>> GetBafSqlSteps =
                CompiledQuery.Compile((DataContext db, UInt64? id) => db.GetTable<BafSqlStep>().Where(x => x.TargetSpectrum == id));

        public Func<DataContext, UInt64?, IEnumerable<BafSqlPerSpectrumVariable>> GetPerSpectrumVariables =
                CompiledQuery.Compile((DataContext db, UInt64? id) => db.GetTable<BafSqlPerSpectrumVariable>().Where(x => x.Spectrum == id && x.Variable != null && x.Value != null));
         
        #region IDisposable Members

        public void Dispose()
        {
            if (isDisposed)
                return;
            
            if (core != null)
                core.Dispose();

            isDisposed = true;
        }

        #endregion
    }

    [Table(Name = "Spectra")]
    internal sealed class BafSqlSpectrum
    {
        [Column(IsPrimaryKey = true)]
        public Nullable<UInt64> Id { get; set; }
        [Column]
        public Nullable<double> Rt { get; set; }
        [Column]
        public Nullable<UInt64> Segment { get; set; }
        [Column]
        public Nullable<UInt64> AcquisitionKey { get; set; }
        [Column]
        public Nullable<UInt64> Parent { get; set; }
        [Column]
        public Nullable<UInt64> MzAcqRangeLower { get; set; }
        [Column]
        public Nullable<UInt64> MzAcqRangeUpper { get; set; }
        [Column]
        public Nullable<double> SumIntensity { get; set; }
        [Column]
        public Nullable<double> MaxIntensity { get; set; }
        [Column]
        public Nullable<UInt64> TransformatorId { get; set; }
        [Column]
        public Nullable<UInt64> ProfileMzId { get; set; }
        [Column]
        public Nullable<UInt64> ProfileIntensityId { get; set; }
        [Column]
        public Nullable<UInt64> LineIndexId { get; set; }
        [Column]
        public Nullable<UInt64> LineMzId { get; set; }
        [Column]
        public Nullable<UInt64> LineIntensityId { get; set; }
        [Column]
        public Nullable<UInt64> LineIndexWidthId { get; set; }
        [Column]
        public Nullable<UInt64> LinePeakAreaId { get; set; }
        [Column]
        public Nullable<UInt64> LineSnrId { get; set; }
    }

    [Table(Name = "AcquisitionKeys")]
    internal sealed class BafSqlAcquisitionKey
    {
        [Column(IsPrimaryKey = true)]
        public Nullable<UInt64> Id { get; set; }
        [Column]
        public Nullable<int> Polarity { get; set; }
        [Column]
        public Nullable<int> ScanMode { get; set; }
        [Column]
        public Nullable<int> AcquisitionMode { get; set; }
        [Column]
        public Nullable<int> MsLevel { get; set; }
    }

    [Table(Name = "PerSpectrumVariables")]
    internal sealed class BafSqlPerSpectrumVariable
    {
        [Column(IsPrimaryKey = true)]
        public Nullable<UInt64> Spectrum { get; set; }
        [Column(IsPrimaryKey = true)]
        public Nullable<UInt64> Variable { get; set; }
        [Column]
        public Nullable<decimal> Value { get; set; }
    }

    [Table(Name = "SupportedVariables")]
    internal sealed class BafSqlSupportedVariable
    {
        [Column(IsPrimaryKey = true)]
        public Nullable<UInt64> Variable { get; set; }
        [Column]
        public string PermanentName { get; set; }
        [Column]
        public Nullable<UInt64> Type { get; set; }
        [Column]
        public string DisplayGroupName { get; set; }
        [Column]
        public string DisplayName { get; set; }
        [Column]
        public string DisplayValueText { get; set; }
        [Column]
        public string DisplayFormat { get; set; }
        [Column]
        public string DisplayDimension { get; set; }
    }

    [Table(Name = "Steps")]
    internal sealed class BafSqlStep
    {
        [Column]
        public Nullable<UInt64> TargetSpectrum { get; set; }
        [Column]
        public Nullable<int> Number { get; set; }
        [Column]
        public Nullable<int> IsolationType { get; set; }
        [Column]
        public Nullable<int> ReactionType { get; set; }
        [Column]
        public Nullable<int> MsLevel { get; set; }
        [Column]
        public Nullable<double> Mass { get; set; }
    }
}
