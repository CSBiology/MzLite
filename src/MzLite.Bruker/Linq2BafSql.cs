﻿#region license
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
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;

namespace MzLite.Bruker
{
    internal sealed class Linq2BafSql : DataContext
    {

        private static readonly AttributeMappingSource mapping = new AttributeMappingSource();

        public Linq2BafSql(IDbConnection connection)
            : base(connection, mapping)
        {
            this.DeferredLoadingEnabled = false;
            this.ObjectTrackingEnabled = false;
        }

        public IQueryable<BafSqlSpectrum> Spectra { get { return base.GetTable<BafSqlSpectrum>(); } }

        public IQueryable<BafSqlAcquisitionKey> AcquisitionKeys { get { return base.GetTable<BafSqlAcquisitionKey>(); } }

        public IQueryable<BafSqlPerSpectrumVariable> PerSpectrumVariables { get { return base.GetTable<BafSqlPerSpectrumVariable>(); } }

        public IQueryable<BafSqlSupportedVariable> SupportedVariables { get { return base.GetTable<BafSqlSupportedVariable>(); } }

        public IQueryable<BafSqlStep> Steps { get { return base.GetTable<BafSqlStep>(); } }        
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