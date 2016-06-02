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

    public interface IParamEdit
    {

        IHasValue SetCvParam(string accession);        
        bool HasCvParam(string accession);
        IValueConverter GetCvParam(string accession);

        IHasValue SetUserParam(string name);        
        bool HasUserParam(string name);
        IValueConverter GetUserParam(string name);
    }

    public interface IHasUnit
    {
        IParamEdit SetUnit(string accession);
        IParamEdit NoUnit();
    }

    public interface IHasValue
    {
        IHasUnit SetValue(IConvertible value);
        IHasUnit NoValue();        
    }

    public interface IValueConverter
    {
        string GetUnit();
        bool HasUnit();
        bool HasUnit(string accession);
        bool HasValue();
        IConvertible GetValue();
        string GetString();
        bool? GetBoolean();
        byte? GetByte();
        char? GetChar();
        double? GetDouble();
        int? GetInt32();        
        long? GetInt64();
        float? GetSingle();        
    }

    public class ParamEdit : IParamEdit
    {

        public static CultureInfo FormatProvider = new CultureInfo("en-US");

        internal ParamEdit(IParamContainer pc)
        {
            this.ParamContainer = pc;
        }

        public IParamContainer ParamContainer { get; private set; }
        internal ParamBase Param { get; private set; }

        #region IParamEdit Members
        
        public IHasValue SetCvParam(string accession)
        {
            
            if (string.IsNullOrWhiteSpace(accession))
                throw new ArgumentNullException("accession");

            if (HasCvParam(accession))
            {
                Param = ParamContainer.CvParams[accession];
            }
            else
            {
                CvParam param = new CvParam(accession);
                ParamContainer.CvParams.Add(param);
                Param = param;
            }

            return new HasValue(this);
        }

        public bool HasCvParam(string accession)
        {
            return ParamContainer.CvParams.Contains(accession);
        }

        public IValueConverter GetCvParam(string accession)
        {
            if (string.IsNullOrWhiteSpace(accession))
                throw new ArgumentNullException("accession");

            if (HasCvParam(accession))
            {
                return new ValueConverter(ParamContainer.CvParams[accession]);
            }
            else
            {
                return new ValueConverter(null);
            }            
        }

        public IHasValue SetUserParam(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            if (HasUserParam(name))
            {
                Param = ParamContainer.UserParams[name];                
            }
            else
            {
                UserParam param = new UserParam(name);
                ParamContainer.UserParams.Add(param);
                Param = param;
            }

            return new HasValue(this);
        }

        public bool HasUserParam(string name)
        {
            return ParamContainer.UserParams.Contains(name);
        }
        
        public IValueConverter GetUserParam(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            if (HasUserParam(name))
            {
                return new ValueConverter(ParamContainer.UserParams[name]);
            }
            else
            {
                return new ValueConverter(null);
            }  
        }

        #endregion

        internal class HasUnit : IHasUnit
        {

            private readonly ParamEdit paramEdit;

            public HasUnit(ParamEdit pe)
            {
                paramEdit = pe;
            }

            #region IHasUnit Members

            public IParamEdit SetUnit(string accession)
            {
                
                if (string.IsNullOrWhiteSpace(accession))
                    throw new ArgumentNullException("accession");                

                paramEdit.Param.CvUnitAccession = accession;

                return paramEdit;
            }

            public IParamEdit NoUnit()
            {
                paramEdit.Param.CvUnitAccession = null;
                return paramEdit;
            }

            #endregion
        }

        internal class HasValue : IHasValue
        {
            private readonly ParamEdit paramEdit;

            public HasValue(ParamEdit pe)
            {
                paramEdit = pe;
            }

            #region IHasValue Members

            public IHasUnit SetValue(IConvertible value)
            {
                paramEdit.Param.Value = value;
                return new HasUnit(paramEdit);
            }

            public IHasUnit NoValue()
            {
                paramEdit.Param.Value = null;
                return new HasUnit(paramEdit);
            }

            #endregion
        }

        internal class ValueConverter : IValueConverter
        {

            private readonly ParamBase param;

            public ValueConverter(ParamBase p) 
            {
                this.param = p;
            }

            #region IValueConverter Members

            public string GetUnit()
            {
                if (param == null)
                    return null;
                else
                    return param.CvUnitAccession;
            }

            public bool HasUnit()
            {
                if (param == null)
                    return false;
                else
                    return string.IsNullOrWhiteSpace(param.CvUnitAccession) == false;
            }

            public bool HasUnit(string accession)
            {
                if (param == null)
                    return false;
                else
                    return param.CvUnitAccession.Equals(accession, StringComparison.InvariantCultureIgnoreCase);
            }

            public bool HasValue()
            {
                if (param == null)
                    return false;
                else
                    return param.Value != null;
            }

            public IConvertible GetValue()
            {
                if (HasValue())
                    return param.Value;
                else
                    return null;
            }

            public string GetString()
            {
                if (HasValue())
                    return param.Value.ToString(ParamEdit.FormatProvider);
                else
                    return default(string);
            }

            public bool? GetBoolean()
            {
                if (HasValue())
                    return param.Value.ToBoolean(ParamEdit.FormatProvider);
                else
                    return default(Nullable<Boolean>);
            }

            public byte? GetByte()
            {
                if (HasValue())
                    return param.Value.ToByte(ParamEdit.FormatProvider);
                else
                    return default(Nullable<byte>);
            }

            public char? GetChar()
            {
                if (HasValue())
                    return param.Value.ToChar(ParamEdit.FormatProvider);
                else
                    return default(Nullable<char>);
            }

            public double? GetDouble()
            {
                if (HasValue())
                    return param.Value.ToDouble(ParamEdit.FormatProvider);
                else
                    return default(Nullable<double>);
            }

            public int? GetInt32()
            {
                if (HasValue())
                    return param.Value.ToInt32(ParamEdit.FormatProvider);
                else
                    return default(Nullable<int>);
            }

            public long? GetInt64()
            {
                if (HasValue())
                    return param.Value.ToInt64(ParamEdit.FormatProvider);
                else
                    return default(Nullable<long>);
            }

            public float? GetSingle()
            {
                if (HasValue())
                    return param.Value.ToSingle(ParamEdit.FormatProvider);
                else
                    return default(Nullable<float>);
            }

            #endregion
        }        
    }

    public static class ParamEditExtension
    {        
        public static IParamEdit BeginParamEdit(
            this IParamContainer pc)
        {
            return new ParamEdit(pc);
        }        
    }
}
