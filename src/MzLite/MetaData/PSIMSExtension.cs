#region license
// The MIT License (MIT)

// PSIMSExtension.cs

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
using MzLite.Model;
using MzLite.MetaData.UO;
using MzLite.Binary;

namespace MzLite.MetaData.PSIMS
{

    public static class PSIMS_Units
    {

        public const string Mz = "MS:1000040";
        public const string NumberOfCounts = "MS:1000131";

        public static TPC PSIMS_Mz<TPC>(this IHasUnit<TPC> attr) where TPC : IParamContainer
        {
            return attr.SetUnit(Mz);
        }

        public static TPC PSIMS_NumberOfCounts<TPC>(this IHasUnit<TPC> attr) where TPC : IParamContainer
        {
            return attr.SetUnit(NumberOfCounts);
        }

    }

    public static class PSIMS_Spectrum
    {

        public const string MsLevel = "MS:1000511";
        public const string CentroidSpectrum = "MS:1000127";
        public const string ProfileSpectrum = "MS:1000128";

        /// <summary>
        /// Stages of ms achieved in a multi stage mass spectrometry experiment. [PSI:MS]
        /// </summary>
        public static MassSpectrum SetMsLevel(
            this MassSpectrum ms, int level)
        {
            if (level < 1)
                throw new ArgumentOutOfRangeException("level");
            return ms.SetCvParam(MsLevel, level).NoUnit();
        }

        /// <summary>
        /// Stages of ms achieved in a multi stage mass spectrometry experiment. [PSI:MS]
        /// </summary>
        public static bool TryGetMsLevel(this MassSpectrum ms, out int msLevel)
        {

            CvParam p;
            if (ms.TryGetParam(MsLevel, out p))
            {
                msLevel = p.GetInt32();
                return true;
            }
            else
            {
                msLevel = default(int);
                return false;
            }
        }

        /// <summary>
        /// Processing of profile data to produce spectra that contains discrete peaks of zero width. 
        /// Often used to reduce the size of dataset. [PSI:MS]
        /// </summary>        
        public static MassSpectrum SetCentroidSpectrum(
            this MassSpectrum ms)
        {
            return ms.SetCvParam(CentroidSpectrum).NoUnit();
        }

        /// <summary>
        /// A profile mass spectrum is created when data is recorded with ion current (counts per second) 
        /// on one axis and mass/charge ratio on another axis. [PSI:MS]
        /// </summary>        
        public static MassSpectrum SetProfileSpectrum(
            this MassSpectrum ms)
        {
            return ms.SetCvParam(ProfileSpectrum).NoUnit();
        }

    }

    public static class PSIMS_IsolationWindow
    {

        public const string IsolationWindowTargetMz = "MS:1000827";
        public const string IsolationWindowLowerOffset = "MS:1000828";
        public const string IsolationWindowUpperOffset = "MS:1000829";

        /// <summary>
        /// The primary or reference m/z about which the isolation window is defined. [PSI:MS]
        /// </summary>        
        public static IsolationWindow SetIsolationWindowTargetMz(
            this IsolationWindow iw, double mz)
        {
            if (mz < 0)
                throw new ArgumentOutOfRangeException("mz");
            return iw.SetCvParam(IsolationWindowTargetMz, mz).PSIMS_Mz();
        }

        /// <summary>
        /// The primary or reference m/z about which the isolation window is defined. [PSI:MS]
        /// </summary>
        public static bool TryGetIsolationWindowTargetMz(
            this IsolationWindow iw, out double mz)
        {
            CvParam p;
            if (iw.TryGetParam(IsolationWindowTargetMz, out p))
            {
                mz = p.GetDouble();
                return true;
            }
            else
            {
                mz = default(double);
                return false;
            }
        }

        /// <summary>
        /// The extent of the isolation window in m/z below the isolation window target m/z. 
        /// The lower and upper offsets may be asymmetric about the target m/z. [PSI:MS]
        /// </summary>        
        public static IsolationWindow SetIsolationWindowLowerOffset(
            this IsolationWindow iw, double offset)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            return iw.SetCvParam(IsolationWindowLowerOffset, offset).PSIMS_Mz();
        }

        /// <summary>
        /// The extent of the isolation window in m/z below the isolation window target m/z. 
        /// The lower and upper offsets may be asymmetric about the target m/z. [PSI:MS]
        /// </summary>
        public static bool TryGetIsolationWindowLowerOffset(
            this IsolationWindow iw, out double offset)
        {
            CvParam p;
            if (iw.TryGetParam(IsolationWindowLowerOffset, out p))
            {
                offset = p.GetDouble();
                return true;
            }
            else
            {
                offset = default(double);
                return false;
            }
        }

        /// <summary>
        /// The extent of the isolation window in m/z above the isolation window target m/z.
        /// The lower and upper offsets may be asymmetric about the target m/z. [PSI:MS]
        /// </summary>
        public static IsolationWindow SetIsolationWindowUpperOffset(
            this IsolationWindow iw, double offset)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            return iw.SetCvParam(IsolationWindowUpperOffset, offset).PSIMS_Mz();
        }

        /// <summary>
        /// The extent of the isolation window in m/z above the isolation window target m/z.
        /// The lower and upper offsets may be asymmetric about the target m/z. [PSI:MS]
        /// </summary>
        public static bool TryGetIsolationWindowUpperOffset(
            this IsolationWindow iw, out double offset)
        {
            CvParam p;
            if (iw.TryGetParam(IsolationWindowUpperOffset, out p))
            {
                offset = p.GetDouble();
                return true;
            }
            else
            {
                offset = default(double);
                return false;
            }
        }

    }

    public static class PSIMS_Precursor
    {

        public const string SelectedIonMz = "MS:1002234";
        public const string ChargeState = "MS:1000041";
        public const string CollisionEnergy = "MS:1000045";

        /// <summary>
        /// Mass-to-charge ratio of a precursor ion selected for fragmentation. [PSI:PI]
        /// </summary>        
        public static SelectedIon SetSelectedIonMz(
            this SelectedIon si, double mz)
        {
            if (mz < 0)
                throw new ArgumentOutOfRangeException("mz");
            return si.SetCvParam(SelectedIonMz, mz).PSIMS_Mz();
        }

        /// <summary>
        /// Mass-to-charge ratio of a precursor ion selected for fragmentation. [PSI:PI]
        /// </summary>
        public static bool TryGetSelectedIonMz(
            this SelectedIon si, out double mz)
        {
            CvParam p;
            if (si.TryGetParam(SelectedIonMz, out p))
            {
                mz = p.GetDouble();
                return true;
            }
            else
            {
                mz = default(double);
                return false;
            }
        }

        /// <summary>
        /// The charge state of the ion, single or multiple and positive or negatively charged. [PSI:MS]
        /// </summary>        
        public static SelectedIon SetChargeState(
            this SelectedIon si, int state)
        {
            return si.SetCvParam(ChargeState, state).NoUnit();
        }


        /// <summary>
        /// Energy for an ion experiencing collision with a stationary gas particle resulting in dissociation of the ion. [PSI:MS]
        /// </summary>        
        public static Activation SetCollisionEnergy(
            this Activation ac, double ce)
        {
            if (ce < 0)
                throw new ArgumentOutOfRangeException("ce");
            return ac.SetCvParam(CollisionEnergy, ce).UO_Electronvolt();
        }
    }

    public static class PSIMS_Scan
    {

        public const string ScanStartTime = "MS:1000016";
        public const string FilterString = "MS:1000512";

        /// <summary>
        /// The time that an analyzer started a scan, relative to the start of the MS run. [PSI:MS]
        /// </summary>        
        public static IHasUnit<Scan> SetScanStartTime(
            this Scan scan, double value)
        {
            return scan.SetCvParam(ScanStartTime, value);
        }

        /// <summary>
        /// The time that an analyzer started a scan, relative to the start of the MS run. [PSI:MS]
        /// </summary>
        public static bool TryGetScanStartTime(this Scan scan, out double rt)
        {
            CvParam p;
            if (scan.TryGetParam(ScanStartTime, out p))
            {
                rt = p.GetDouble();
                return true;
            }
            else
            {
                rt = default(double);
                return false;
            }
        }

        /// <summary>
        /// A string unique to Thermo instrument describing instrument settings for the scan. [PSI:MS]
        /// </summary>        
        public static Scan SetFilterString(this Scan scan, string filterString)
        {
            if (string.IsNullOrWhiteSpace(filterString))
                filterString = string.Empty;
            return scan.SetCvParam(FilterString, filterString).NoUnit();
        }

        /// <summary>
        /// A string unique to Thermo instrument describing instrument settings for the scan. [PSI:MS]
        /// </summary>
        public static bool TryGetFilterString(this Scan scan, out string filterString)
        {
            CvParam p;
            if (scan.TryGetParam(FilterString, out p))
            {
                filterString = p.GetString();
                return true;
            }
            else
            {
                filterString = default(string);
                return false;
            }
        }

    }

    public static class PSIMS_BinaryDataArray
    {

        /// <summary>
        /// A data array of m/z values. [PSI:MS]
        /// </summary>        
        public static IParamContainer SetMzArray(
            this IParamContainer pc)
        {
            return pc.SetCvParam("MS:1000514").PSIMS_Mz();
        }

        public static bool IsMzArray(
            this IParamContainer pc)
        {
            return pc.HasCvParam("MS:1000514");
        }

        /// <summary>
        /// A data array of intensity values. [PSI:MS]
        /// </summary>        
        public static IHasUnit<TPC> SetIntensityArray<TPC>(
            this TPC pc) where TPC : IParamContainer
        {
            return pc.SetCvParam("MS:1000515");
        }

        public static bool IsIntensityArray(
            this IParamContainer pc)
        {
            return pc.HasCvParam("MS:1000515");
        }

        /// <summary>
        /// A data array of relative time offset values from a reference time. [PSI:MS]
        /// </summary>        
        public static IHasUnit<TPC> MS_TimeArray<TPC>(
            this TPC pc) where TPC : IParamContainer
        {
            return pc.SetCvParam("MS:1000595");
        }

        public static bool IsTimeArray(
            this IParamContainer pc)
        {
            return pc.HasCvParam("MS:1000595");
        }

        /// <summary>
        /// No Compression. [PSI:MS]
        /// </summary>        
        public static IParamContainer SetNoCompression(
            this IParamContainer pc)
        {
            return pc.SetCvParam("MS:1000576").NoUnit();
        }

        public static bool IsNoCompression(
            this IParamContainer pc)
        {
            return pc.HasCvParam("MS:1000576");
        }

        /// <summary>
        /// Zlib (gzip) Compression. [PSI:MS]
        /// </summary>        
        public static IParamContainer SetZlibCompression(
            this IParamContainer pc)
        {
            return pc.SetCvParam("MS_1000574").NoUnit();
        }

        public static bool IsZlibCompression(
            this IParamContainer pc)
        {
            return pc.HasCvParam("MS_1000574");
        }

        /// <summary>
        /// Compression using MS-Numpress linear prediction compression. [https://github.com/fickludd/ms-numpress]
        /// </summary>        
        public static IParamContainer SetMSNumpressLinearPredictionCompression(
            this IParamContainer pc)
        {
            return pc.SetCvParam("MS:1002312").NoUnit();
        }

        /// <summary>
        /// Compression using MS-Numpress positive integer compression. [https://github.com/fickludd/ms-numpress]
        /// </summary>        
        public static IParamContainer SetMSNumpressPositiveIntegerCompression(
            this IParamContainer pc)
        {
            return pc.SetCvParam("MS:1002313").NoUnit();
        }

        /// <summary>
        /// Compression using MS-Numpress short logged float compression. [https://github.com/fickludd/ms-numpress]
        /// </summary>        
        public static IParamContainer SetMSNumpressShortLoggedFloatCompression(
            this IParamContainer pc)
        {
            return pc.SetCvParam("MS:1002314").NoUnit();
        }

        public static IParamContainer SetCompression(
            this IParamContainer pc, 
            BinaryDataCompressionType compressionType)
        {
            switch (compressionType)
            {
                case BinaryDataCompressionType.NoCompression:
                    return pc.SetNoCompression();
                case BinaryDataCompressionType.ZLib:
                    return pc.SetZlibCompression();                    
                default:
                    throw new NotSupportedException("Compression type not supported: " + compressionType.ToString());
            }
        }


        /// <summary>
        /// 64-bit precision little-endian floating point conforming to IEEE-754. [PSI:MS]
        /// </summary>        
        public static IParamContainer Set64BitFloat(
            this IParamContainer pc)
        {
            return pc.SetCvParam("MS:1000523").NoUnit();
        }

        public static bool Is64BitFloat(
            this IParamContainer pc)
        {
            return pc.HasCvParam("MS:1000523");
        }

        /// <summary>
        /// 32-bit precision little-endian floating point conforming to IEEE-754. [PSI:MS]
        /// </summary>        
        public static IParamContainer Set32BitFloat(
            this IParamContainer pc)
        {
            return pc.SetCvParam("MS:1000521").NoUnit();
        }

        public static bool Is32BitFloat(
            this IParamContainer pc)
        {
            return pc.HasCvParam("MS:1000521");
        }

        /// <summary>
        /// Signed 64-bit little-endian integer. [PSI:MS]
        /// </summary>        
        public static IParamContainer Set64BitInteger(
            this IParamContainer pc)
        {
            return pc.SetCvParam("MS:1000522").NoUnit();
        }

        public static bool Is64BitInteger(
            this IParamContainer pc)
        {
            return pc.HasCvParam("MS:1000522");
        }

        /// <summary>
        /// Signed 32-bit little-endian integer. [PSI:MS]
        /// </summary>        
        public static IParamContainer Set32BitInteger(
            this IParamContainer pc)
        {
            return pc.SetCvParam("MS:1000519").NoUnit();
        }

        public static bool Is32BitInteger(
            this IParamContainer pc)
        {
            return pc.HasCvParam("MS:1000519");
        }

        public static IParamContainer SetBinaryDataType(
            this IParamContainer pc,
            BinaryDataType binaryDataType)
        {
            switch (binaryDataType)
            {
                case BinaryDataType.Float32:
                    return pc.Set32BitFloat();
                case BinaryDataType.Float64:
                    return pc.Set64BitFloat();
                case BinaryDataType.Int32:
                    return pc.Set32BitInteger();
                case BinaryDataType.Int64:
                    return pc.Set64BitInteger();
                default:
                    throw new NotSupportedException("Data type not supported: " + binaryDataType.ToString());
            }
        }

    }

    //public static class PSIMS
    //{

    //    #region sample

    //    public static IParamContainer MS_SampleNumber(
    //        this IParamContainer pc,
    //        string value)
    //    {
    //        return pc.SetCvParam("MS:1000001", value).NoUnit();
    //    }

    //    public static IParamContainer MS_SampleState(
    //        this IParamContainer pc, string value)
    //    {
    //        return pc.SetCvParam("MS:1000003", value).NoUnit();
    //    }

    //    public static IParamContainer MS_SampleBatch(
    //        this IParamContainer pc, string value)
    //    {
    //        return pc.SetCvParam("MS:1000053", value).NoUnit();
    //    }

    //    public static IHasUnit MS_SampleConcentration(
    //        this IParamContainer pc, double value)
    //    {
    //        return pc.SetCvParam("MS:1000006", value);
    //    }

    //    public static IHasUnit MS_SampleVolume(
    //        this IParamContainer pc, double value)
    //    {
    //        return pc.SetCvParam("MS:1000005", value);
    //    }

    //    public static IHasUnit MS_SampleMass(
    //        this IParamContainer pc, double value)
    //    {
    //        return pc.SetCvParam("MS:1000004", value);
    //    }

    //    public static IParamContainer MS_GenericExperimentalCondition(
    //        this IParamContainer pc, string value)
    //    {
    //        return pc.SetCvParam("MS:1001814", value).NoUnit();
    //    }

    //    #endregion

    //    #region raw files

    //    public static IParamContainer MS_ThermoRawFormat(
    //        this IParamContainer pc)
    //    {
    //        return pc.SetCvParam("MS:1000563").NoUnit();
    //    }

    //    public static IParamContainer MS_ABIWIFFFormat(
    //        this IParamContainer pc)
    //    {
    //        return pc.SetCvParam("MS:1000562").NoUnit();
    //    }

    //    /// <summary>
    //    /// Native format defined by sample=xsd:nonNegativeInteger period=xsd:nonNegativeInteger cycle=xsd:nonNegativeInteger experiment=xsd:nonNegativeInteger. [PSI:MS]
    //    /// </summary>
    //    /// <param name="pc"></param>
    //    /// <returns></returns>
    //    public static IParamContainer MS_WIFFNativeIDFormat(
    //        this IParamContainer pc)
    //    {
    //        return pc.SetCvParam("MS:1000770").NoUnit();
    //    }

    //    #endregion

    //    #region publication
    //    public static IParamContainer MS_PubMedID(
    //        this IParamContainer pc,
    //        string id)
    //    {
    //        return pc.SetCvParam("MS:1000879",id).NoUnit();
    //    }

    //    public static IParamContainer MS_ContactName(
    //        this IParamContainer pc,
    //        string name)
    //    {
    //        return pc.SetCvParam("MS:1000586",name).NoUnit();
    //    }

    //    public static IParamContainer MS_FirstAuthor(
    //        this IParamContainer pc)
    //    {
    //        return pc.SetCvParam("MS:1002034").NoUnit();
    //    }

    //    public static IParamContainer MS_CoAuthor(
    //        this IParamContainer pc)
    //    {
    //        return pc.SetCvParam("MS:1002036").NoUnit();
    //    }
    //    #endregion

    //    #region spectrum        

    //    /// <summary>
    //    /// A plot of the relative abundance of a beam or other collection of ions as a function of the mass-to-charge ratio (m/z). [MS:MS]
    //    /// </summary>        
    //    /// <param name="pc"></param>
    //    /// <returns></returns>
    //    public static IParamContainer MS_MassSpectrum(
    //        this IParamContainer pc)
    //    {
    //        return pc.SetCvParam("MS:1000294").NoUnit();
    //    }

    //    /// <summary>
    //    /// Processing of profile data to produce spectra that contains discrete peaks of zero width. 
    //    /// Often used to reduce the size of dataset. [PSI:MS]
    //    /// </summary>
    //    /// <param name="pc"></param>
    //    /// <returns></returns>
    //    public static IParamContainer MS_CentroidSpectrum(
    //        this IParamContainer pc)
    //    {
    //        return pc.SetCvParam("MS:1000127").NoUnit();
    //    }

    //    /// <summary>
    //    /// A profile mass spectrum is created when data is recorded with ion current (counts per second) 
    //    /// on one axis and mass/charge ratio on another axis. [PSI:MS]
    //    /// </summary>
    //    /// <param name="pc"></param>
    //    /// <returns></returns>
    //    public static IParamContainer MS_ProfileSpectrum(
    //        this IParamContainer pc)
    //    {
    //        return pc.SetCvParam("MS:1000128").NoUnit();
    //    }

    //    /// <summary>
    //    /// Lowest m/z value observed in the m/z array. [PSI:MS]
    //    /// </summary>
    //    /// <param name="pc"></param>
    //    /// <param name="value"></param>
    //    /// <returns></returns>
    //    public static IHasUnit MS_LowestObservedMz(
    //        this IParamContainer pc, double value)
    //    {
    //        return pc.SetCvParam("MS:1000528", value);
    //    }

    //    /// <summary>
    //    /// Heighest m/z value observed in the m/z array. [PSI:MS]
    //    /// </summary>
    //    /// <param name="pc"></param>
    //    /// <param name="value"></param>
    //    /// <returns></returns>
    //    public static IHasUnit MS_HighestObservedMz(
    //        this IParamContainer pc, double value)
    //    {
    //        return pc.SetCvParam("MS:1000527", value);
    //    }

    //    /// <summary>
    //    /// The intensity of the greatest peak in the mass spectrum. [PSI:MS]
    //    /// </summary>
    //    /// <param name="pc"></param>
    //    /// <param name="value"></param>
    //    /// <returns></returns>
    //    public static IHasUnit MS_BasePeakIntensity(
    //        this IParamContainer pc, double value)
    //    {
    //        return pc.SetCvParam("MS:1000505", value);
    //    }

    //    /// <summary>
    //    /// M/z value of the signal of highest intensity in the mass spectrum. [PSI:MS]
    //    /// </summary>
    //    /// <param name="pc"></param>
    //    /// <param name="value"></param>
    //    /// <returns></returns>
    //    public static IHasUnit MS_BasePeakMz(
    //        this IParamContainer pc, double value)
    //    {
    //        return pc.SetCvParam("MS:1000504", value);
    //    }

    //    /// <summary>
    //    /// The sum of all the separate ion currents carried by the ions of different m/z contributing 
    //    /// to a complete mass spectrum or in a specified m/z range of a mass spectrum. [PSI:MS]
    //    /// </summary>
    //    /// <param name="pc"></param>
    //    /// <param name="value"></param>
    //    /// <returns></returns>
    //    public static IParamContainer MS_TotalIonCurrent(
    //        this IParamContainer pc, double value)
    //    {
    //        return pc.SetCvParam("MS:1000285", value).NoUnit();
    //    }

    //    #endregion

    //    #region binary data

    //    /// <summary>
    //    /// A data array of m/z values. [PSI:MS]
    //    /// </summary>
    //    /// <param name="pc"></param>
    //    /// <returns></returns>
    //    public static IParamContainer MS_MzArray(
    //        this IParamContainer pc)
    //    {
    //        return pc.SetCvParam("MS:1000514").MS_Mz();
    //    }

    //    public static bool Is_MS_MzArray(
    //        this IParamContainer pc)
    //    {
    //        return pc.HasCvParam("MS:1000514");
    //    }

    //    /// <summary>
    //    /// A data array of intensity values. [PSI:MS]
    //    /// </summary>
    //    /// <param name="pc"></param>
    //    /// <returns></returns>
    //    public static IHasUnit MS_IntensityArray(
    //        this IParamContainer pc)
    //    {
    //        return pc.SetCvParam("MS:1000515");
    //    }

    //    public static bool Is_MS_IntensityArray(
    //        this IParamContainer pc)
    //    {
    //        return pc.HasCvParam("MS:1000515");
    //    }

    //    /// <summary>
    //    /// A data array of relative time offset values from a reference time. [PSI:MS]
    //    /// </summary>
    //    /// <param name="pc"></param>
    //    /// <returns></returns>
    //    public static IHasUnit MS_TimeArray(
    //        this IParamContainer pc)
    //    {
    //        return pc.SetCvParam("MS:1000595");
    //    }

    //    public static bool Is_MS_TimeArray(
    //        this IParamContainer pc)
    //    {
    //        return pc.HasCvParam("MS:1000595");
    //    }

    //    /// <summary>
    //    /// No Compression. [PSI:MS]
    //    /// </summary>
    //    /// <param name="pc"></param>
    //    /// <returns></returns>
    //    public static IParamContainer MS_NoCompression(
    //        this IParamContainer pc)
    //    {
    //        return pc.SetCvParam("MS:1000576").NoUnit();
    //    }

    //    public static bool Is_MS_NoCompression(
    //        this IParamContainer pc)
    //    {
    //        return pc.HasCvParam("MS:1000576");
    //    }

    //    /// <summary>
    //    /// Zlib (gzip) Compression. [PSI:MS]
    //    /// </summary>
    //    /// <param name="pc"></param>
    //    /// <returns></returns>
    //    public static IParamContainer MS_ZlibCompression(
    //        this IParamContainer pc)
    //    {
    //        return pc.SetCvParam("MS_1000574").NoUnit();
    //    }

    //    public static bool Is_MS_ZlibCompression(
    //        this IParamContainer pc)
    //    {
    //        return pc.HasCvParam("MS_1000574");
    //    }

    //    /// <summary>
    //    /// Compression using MS-Numpress linear prediction compression. [https://github.com/fickludd/ms-numpress]
    //    /// </summary>
    //    /// <param name="pc"></param>
    //    /// <returns></returns>
    //    public static IParamContainer MS_MSNumpressLinearPredictionCompression(
    //        this IParamContainer pc)
    //    {
    //        return pc.SetCvParam("MS:1002312").NoUnit();
    //    }

    //    /// <summary>
    //    /// Compression using MS-Numpress positive integer compression. [https://github.com/fickludd/ms-numpress]
    //    /// </summary>
    //    /// <param name="pc"></param>
    //    /// <returns></returns>
    //    public static IParamContainer MS_MSNumpressPositiveIntegerCompression(
    //        this IParamContainer pc)
    //    {
    //        return pc.SetCvParam("MS:1002313").NoUnit();
    //    }

    //    /// <summary>
    //    /// Compression using MS-Numpress short logged float compression. [https://github.com/fickludd/ms-numpress]
    //    /// </summary>
    //    /// <param name="pc"></param>
    //    /// <returns></returns>
    //    public static IParamContainer MS_MSNumpressShortLoggedFloatCompression(
    //        this IParamContainer pc)
    //    {
    //        return pc.SetCvParam("MS:1002314").NoUnit();
    //    }

    //    /// <summary>
    //    /// 64-bit precision little-endian floating point conforming to IEEE-754. [PSI:MS]
    //    /// </summary>
    //    /// <param name="pc"></param>
    //    /// <returns></returns>
    //    public static IParamContainer MS_64BitFloat(
    //        this IParamContainer pc)
    //    {
    //        return pc.SetCvParam("MS:1000523").NoUnit();
    //    }

    //    public static bool Is_MS_64BitFloat(
    //        this IParamContainer pc)
    //    {
    //        return pc.HasCvParam("MS:1000523");
    //    }

    //    /// <summary>
    //    /// 32-bit precision little-endian floating point conforming to IEEE-754. [PSI:MS]
    //    /// </summary>
    //    /// <param name="pc"></param>
    //    /// <returns></returns>
    //    public static IParamContainer MS_32BitFloat(
    //        this IParamContainer pc)
    //    {
    //        return pc.SetCvParam("MS:1000521").NoUnit();
    //    }

    //    public static bool Is_MS_32BitFloat(
    //        this IParamContainer pc)
    //    {
    //        return pc.HasCvParam("MS:1000521");
    //    }

    //    /// <summary>
    //    /// Signed 64-bit little-endian integer. [PSI:MS]
    //    /// </summary>
    //    /// <param name="pc"></param>
    //    /// <returns></returns>
    //    public static IParamContainer MS_64BitInteger(
    //        this IParamContainer pc)
    //    {
    //        return pc.SetCvParam("MS:1000522").NoUnit();
    //    }

    //    public static bool Is_MS_64BitInteger(
    //        this IParamContainer pc)
    //    {
    //        return pc.HasCvParam("MS:1000522");
    //    }

    //    /// <summary>
    //    /// Signed 32-bit little-endian integer. [PSI:MS]
    //    /// </summary>
    //    /// <param name="pc"></param>
    //    /// <returns></returns>
    //    public static IParamContainer MS_32BitInteger(
    //        this IParamContainer pc)
    //    {
    //        return pc.SetCvParam("MS:1000519").NoUnit();
    //    }

    //    public static bool Is_MS_32BitInteger(
    //        this IParamContainer pc)
    //    {
    //        return pc.HasCvParam("MS:1000519");
    //    }

    //    #endregion                

    //    #region identification        

    //    /// <summary>
    //    /// The theoretical mass of the molecule (e.g. the peptide sequence and its modifications). [MS:PI]
    //    /// </summary>        
    //    /// <param name="pc"></param>
    //    /// <param name="mass"></param>
    //    /// <returns></returns>
    //    public static IParamContainer MS_TheoreticalMass(
    //        this IParamContainer pc, double mass)
    //    {
    //        return pc.SetCvParam("MS:1001117", mass).UO_Dalton();
    //    }

    //    /// <summary>
    //    /// Andromeda is a peptide search engine. [MS:PI]
    //    /// </summary>        
    //    /// <param name="pc"></param>
    //    /// <returns></returns>
    //    public static IParamContainer MS_Andromeda(
    //        this IParamContainer pc)
    //    {
    //        return pc.SetCvParam("MS:1002337").NoUnit();
    //    }

    //    /// <summary>
    //    /// The probability based score of the Andromeda search engine. [MS:PI]
    //    /// </summary>        
    //    /// <param name="pc"></param>
    //    /// <param name="score"></param>
    //    /// <returns></returns>
    //    public static IParamContainer MS_AndromedaScore(
    //        this IParamContainer pc, double score)
    //    {
    //        return pc.SetCvParam("MS:1002338", score).NoUnit();
    //    }

    //    /// <summary>
    //    /// MaxQuant is a quantitative proteomics software package designed for analyzing large mass spectrometric data sets. 
    //    /// It is specifically aimed at high resolution MS data. [MS:MS]
    //    /// </summary>        
    //    /// <param name="pc"></param>
    //    /// <returns></returns>
    //    public static IParamContainer MS_MaxQuant(
    //        this IParamContainer pc)
    //    {
    //        return pc.SetCvParam("MS:1001583").NoUnit();
    //    }

    //    /// <summary>
    //    /// The data type PEP (posterior error probability) produced by MaxQuant. [MS:MS]
    //    /// </summary>        
    //    /// <param name="pc"></param>
    //    /// <param name="pep"></param>
    //    /// <returns></returns>
    //    public static IParamContainer MS_MaxQuantPEP(
    //        this IParamContainer pc, double pep)
    //    {
    //        return pc.SetCvParam("MS:1001901", pep).NoUnit();
    //    }

    //    /// <summary>
    //    /// The data type feature intensity produced by MaxQuant. [MS:MS]
    //    /// </summary>        
    //    /// <param name="pc"></param>
    //    /// <param name="pep"></param>
    //    /// <returns></returns>
    //    public static IParamContainer MS_MaxQuantFeatureIntensity(
    //        this IParamContainer pc, double intensity)
    //    {
    //        return pc.SetCvParam("MS:1001903", intensity).NoUnit();
    //    }

    //    #endregion
    //}

}
