#region license
// The MIT License (MIT)

// ParamEdit.cs

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
using System.Globalization;
using MzLite.Model;

namespace MzLite.MetaData
{

    public interface IHasUnit<TPC> where TPC : IParamContainer
    {
        TPC ParamContainer { get; }
        ParamBase Param { get; }
        TPC SetUnit(string accession);
        TPC NoUnit();
    }

    public sealed class HasUnit<TPC> : IHasUnit<TPC> where TPC : IParamContainer
    {

        private readonly TPC paramContainer;
        private readonly ParamBase param;

        internal HasUnit(TPC pc, ParamBase p)
        {
            this.paramContainer = pc;
            this.param = p;
        }

        #region IHasUnit2 Members

        public TPC ParamContainer
        {
            get { return paramContainer; }
        }

        public ParamBase Param
        {
            get { return param; }
        }

        public TPC SetUnit(string accession)
        {

            if (string.IsNullOrWhiteSpace(accession))
                throw new ArgumentOutOfRangeException("accession may not be null or empty.");
            param.CvUnitAccession = accession;
            return paramContainer;
        }

        public TPC NoUnit()
        {
            param.CvUnitAccession = null;
            return paramContainer;
        }

        #endregion
    }

    public static class ParamEditExtension
    {

        public static readonly CultureInfo FormatProvider = new CultureInfo("en-US");

        public static IHasUnit<TPC> SetCvParam<TPC>(this TPC pc, string accession) where TPC : IParamContainer
        {
            
            if (string.IsNullOrWhiteSpace(accession))
                throw new ArgumentOutOfRangeException("accession may not be null or empty.");

            CvParam param;

            if (pc.TryGetParam(accession, out param))
            {
                param = pc.CvParams[accession];
            }
            else
            {
                param = new CvParam(accession);
                pc.CvParams.Add(param);                
            }

            return new HasUnit<TPC>(pc, param);

        }

        public static IHasUnit<TPC> SetCvParam<TPC>(this TPC pc, string accession, IConvertible value) where TPC : IParamContainer
        {

            if (string.IsNullOrWhiteSpace(accession))
                throw new ArgumentOutOfRangeException("accession may not be null or empty.");

            CvParam param;

            if (pc.TryGetParam(accession, out param))
            {
                param = pc.CvParams[accession];                
            }
            else
            {
                param = new CvParam(accession);
                pc.CvParams.Add(param);
            }

            param.Value = value;

            return new HasUnit<TPC>(pc, param);
        }

        public static bool HasCvParam<TPC>(this TPC pc, string accession) where TPC : IParamContainer
        {

            if (pc == null)
                throw new ArgumentNullException("pc");
            if (accession == null)
                throw new ArgumentNullException("accession");

            return pc.CvParams.Contains(accession);
        }

        public static bool HasUserParam<TPC>(this TPC pc, string name) where TPC : IParamContainer
        {

            if (pc == null)
                throw new ArgumentNullException("pc");
            if (name == null)
                throw new ArgumentNullException("name");

            return pc.UserParams.Contains(name);
        }

        public static bool TryGetParam<TPC>(this TPC pc, string accession, out CvParam param) where TPC : IParamContainer
        {

            if (pc == null)
                throw new ArgumentNullException("pc");
            if (accession == null)
                throw new ArgumentNullException("accession");

            return pc.CvParams.TryGetItemByKey(accession, out param);
        }

        public static bool TryGetParam<TPC>(this TPC pc, string name, out UserParam param) where TPC : IParamContainer
        {

            if (pc == null)
                throw new ArgumentNullException("pc");
            if (name == null)
                throw new ArgumentNullException("name");

            return pc.UserParams.TryGetItemByKey(name, out param);
        }

        public static bool HasValue(this ParamBase p)
        {
            if (p == null)
                throw new ArgumentNullException("p");
            return p.Value != null;
        }

        public static bool HasUnit(this ParamBase p)
        {
            if (p == null)
                throw new ArgumentNullException("p");
            return string.IsNullOrWhiteSpace(p.CvUnitAccession) == false;
        }

        public static bool HasUnit(this ParamBase p, string unitAccession)
        {

            if (p == null)
                throw new ArgumentNullException("p");
            if (unitAccession == null)
                throw new ArgumentNullException("unitAccession");

            return p.CvUnitAccession != null &&
                p.CvUnitAccession.Equals(unitAccession, StringComparison.InvariantCultureIgnoreCase);
        }

        public static IConvertible GetValueOrDefault(this ParamBase p)
        {
            if (p == null)
                throw new ArgumentNullException("p");

            if (p.HasValue())
                return p.Value;
            else
                return null;
        }

        public static string GetStringOrDefault(this ParamBase p)
        {
            if (p == null)
                throw new ArgumentNullException("p");

            if (p.HasValue())
                return p.Value.ToString(ParamEditExtension.FormatProvider);
            else
                return default(string);
        }

        public static bool GetBooleanOrDefault(this ParamBase p)
        {
            if (p == null)
                throw new ArgumentNullException("p");

            if (p.HasValue())
                return p.Value.ToBoolean(ParamEditExtension.FormatProvider);
            else
                return default(Boolean);
        }

        public static byte GetByteOrDefault(this ParamBase p)
        {
            if (p == null)
                throw new ArgumentNullException("p");

            if (p.HasValue())
                return p.Value.ToByte(ParamEditExtension.FormatProvider);
            else
                return default(byte);
        }

        public static char GetCharOrDefault(this ParamBase p)
        {
            if (p == null)
                throw new ArgumentNullException("p");

            if (p.HasValue())
                return p.Value.ToChar(ParamEditExtension.FormatProvider);
            else
                return default(char);
        }

        public static double GetDoubleOrDefault(this ParamBase p)
        {
            if (p == null)
                throw new ArgumentNullException("p");

            if (p.HasValue())
                return p.Value.ToDouble(ParamEditExtension.FormatProvider);
            else
                return default(double);
        }

        public static int GetInt32OrDefault(this ParamBase p)
        {
            if (p == null)
                throw new ArgumentNullException("p");

            if (p.HasValue())
                return p.Value.ToInt32(ParamEditExtension.FormatProvider);
            else
                return default(int);
        }

        public static long GetInt64OrDefault(this ParamBase p)
        {
            if (p == null)
                throw new ArgumentNullException("p");

            if (p.HasValue())
                return p.Value.ToInt64(ParamEditExtension.FormatProvider);
            else
                return default(long);
        }

        public static float GetSingleOrDefault(this ParamBase p)
        {
            if (p == null)
                throw new ArgumentNullException("p");

            if (p.HasValue())
                return p.Value.ToSingle(ParamEditExtension.FormatProvider);
            else
                return default(float);
        }

        public static IConvertible GetValue(this ParamBase p)
        {
            if (p == null)
                throw new ArgumentNullException("p");

            if (p.HasValue())
                return p.Value;
            else
                throw new InvalidOperationException("Param value not set.");
        }

        public static string GetString(this ParamBase p)
        {
            if (p == null)
                throw new ArgumentNullException("p");

            if (p.HasValue())
                return p.Value.ToString(ParamEditExtension.FormatProvider);
            else
                throw new InvalidOperationException("Param value not set.");
        }

        public static bool GetBoolean(this ParamBase p)
        {
            if (p == null)
                throw new ArgumentNullException("p");

            if (p.HasValue())
                return p.Value.ToBoolean(ParamEditExtension.FormatProvider);
            else
                throw new InvalidOperationException("Param value not set.");
        }

        public static byte GetByte(this ParamBase p)
        {
            if (p == null)
                throw new ArgumentNullException("p");

            if (p.HasValue())
                return p.Value.ToByte(ParamEditExtension.FormatProvider);
            else
                throw new InvalidOperationException("Param value not set.");
        }

        public static char GetChar(this ParamBase p)
        {
            if (p == null)
                throw new ArgumentNullException("p");

            if (p.HasValue())
                return p.Value.ToChar(ParamEditExtension.FormatProvider);
            else
                throw new InvalidOperationException("Param value not set.");
        }

        public static double GetDouble(this ParamBase p)
        {
            if (p == null)
                throw new ArgumentNullException("p");

            if (p.HasValue())
                return p.Value.ToDouble(ParamEditExtension.FormatProvider);
            else
                throw new InvalidOperationException("Param value not set.");
        }

        public static int GetInt32(this ParamBase p)
        {
            if (p == null)
                throw new ArgumentNullException("p");

            if (p.HasValue())
                return p.Value.ToInt32(ParamEditExtension.FormatProvider);
            else
                throw new InvalidOperationException("Param value not set.");
        }

        public static long GetInt64(this ParamBase p)
        {
            if (p == null)
                throw new ArgumentNullException("p");

            if (p.HasValue())
                return p.Value.ToInt64(ParamEditExtension.FormatProvider);
            else
                throw new InvalidOperationException("Param value not set.");
        }

        public static float GetSingle(this ParamBase p)
        {
            if (p == null)
                throw new ArgumentNullException("p");

            if (p.HasValue())
                return p.Value.ToSingle(ParamEditExtension.FormatProvider);
            else
                throw new InvalidOperationException("Param value not set.");
        }

    }
}
