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

namespace MzLite.MetaData
{

    public static class PSIMS
    {
        
        #region sample

        public static IParamEdit MS_SampleNumber(
            this IParamEdit etype,
            string value)
        {
            return etype.SetCvParam("MS:1000001").SetValue(value).NoUnit();
        }

        public static IParamEdit MS_SampleState(
            this IParamEdit etype, string value)
        {
            return etype.SetCvParam("MS:1000003").SetValue(value).NoUnit();
        }

        public static IParamEdit MS_SampleBatch(
            this IParamEdit etype, string value)
        {
            return etype.SetCvParam("MS:1000053").SetValue(value).NoUnit();
        }

        public static IHasUnit MS_SampleConcentration(
            this IParamEdit etype, double value)
        {
            return etype.SetCvParam("MS:1000006").SetValue(value);
        }

        public static IHasUnit MS_SampleVolume(
            this IParamEdit etype, double value)
        {
            return etype.SetCvParam("MS:1000005").SetValue(value);
        }

        public static IHasUnit MS_SampleMass(
            this IParamEdit etype, double value)
        {
            return etype.SetCvParam("MS:1000004").SetValue(value);
        }

        public static IParamEdit MS_GenericExperimentalCondition(
            this IParamEdit etype, string value)
        {
            return etype.SetCvParam("MS:1001814").SetValue(value).NoUnit();
        }

        #endregion

        #region raw files

        public static IParamEdit MS_ThermoRawFormat(
            this IParamEdit etype)
        {
            return etype.SetCvParam("MS:1000563").NoValue().NoUnit();
        }

        public static IParamEdit MS_ABIWIFFFormat(
            this IParamEdit etype)
        {
            return etype.SetCvParam("MS:1000562").NoValue().NoUnit();
        }

        /// <summary>
        /// Native format defined by sample=xsd:nonNegativeInteger period=xsd:nonNegativeInteger cycle=xsd:nonNegativeInteger experiment=xsd:nonNegativeInteger. [PSI:MS]
        /// </summary>
        /// <param name="etype"></param>
        /// <returns></returns>
        public static IParamEdit MS_WIFFNativeIDFormat(
            this IParamEdit etype)
        {
            return etype.SetCvParam("MS:1000770").NoValue().NoUnit();
        }

        #endregion

        #region publication
        public static IParamEdit MS_PubMedID(
            this IParamEdit etype,
            string id)
        {
            return etype.SetCvParam("MS:1000879").SetValue(id).NoUnit();
        }

        public static IParamEdit MS_ContactName(
            this IParamEdit etype,
            string name)
        {
            return etype.SetCvParam("MS:1000586").SetValue(name).NoUnit();
        }

        public static IParamEdit MS_FirstAuthor(
            this IParamEdit etype)
        {
            return etype.SetCvParam("MS:1002034").NoValue().NoUnit();
        }

        public static IParamEdit MS_CoAuthor(
            this IParamEdit etype)
        {
            return etype.SetCvParam("MS:1002036").NoValue().NoUnit();
        }
        #endregion

        #region spectrum

        /// <summary>
        /// Stages of ms achieved in a multi stage mass spectrometry experiment. [PSI:MS]
        /// </summary>
        /// <param name="etype"></param>
        /// <returns></returns>
        public static IParamEdit MS_MsLevel(
            this IParamEdit etype, int level)
        {
            if (level < 1)
                throw new ArgumentOutOfRangeException("level");
            return etype.SetCvParam("MS:1000511").SetValue(level).NoUnit();
        }

        /// <summary>
        /// Stages of ms achieved in a multi stage mass spectrometry experiment. [PSI:MS]
        /// </summary>
        public static IValueConverter Get_MS_Level(this IParamEdit etype)
        {
            return etype.GetCvParam("MS:1000511");
        }

        /// <summary>
        /// A plot of the relative abundance of a beam or other collection of ions as a function of the mass-to-charge ratio (m/z). [MS:MS]
        /// </summary>        
        /// <param name="etype"></param>
        /// <returns></returns>
        public static IParamEdit MS_MassSpectrum(
            this IParamEdit etype)
        {
            return etype.SetCvParam("MS:1000294").NoValue().NoUnit();
        }

        /// <summary>
        /// Processing of profile data to produce spectra that contains discrete peaks of zero width. 
        /// Often used to reduce the size of dataset. [PSI:MS]
        /// </summary>
        /// <param name="etype"></param>
        /// <returns></returns>
        public static IParamEdit MS_CentroidSpectrum(
            this IParamEdit etype)
        {
            return etype.SetCvParam("MS:1000127").NoValue().NoUnit();
        }

        /// <summary>
        /// A profile mass spectrum is created when data is recorded with ion current (counts per second) 
        /// on one axis and mass/charge ratio on another axis. [PSI:MS]
        /// </summary>
        /// <param name="etype"></param>
        /// <returns></returns>
        public static IParamEdit MS_ProfileSpectrum(
            this IParamEdit etype)
        {
            return etype.SetCvParam("MS:1000128").NoValue().NoUnit();
        }

        /// <summary>
        /// Lowest m/z value observed in the m/z array. [PSI:MS]
        /// </summary>
        /// <param name="etype"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IHasUnit MS_LowestObservedMz(
            this IParamEdit etype, double value)
        {
            return etype.SetCvParam("MS:1000528").SetValue(value);
        }

        /// <summary>
        /// Heighest m/z value observed in the m/z array. [PSI:MS]
        /// </summary>
        /// <param name="etype"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IHasUnit MS_HighestObservedMz(
            this IParamEdit etype, double value)
        {
            return etype.SetCvParam("MS:1000527").SetValue(value);
        }

        /// <summary>
        /// The intensity of the greatest peak in the mass spectrum. [PSI:MS]
        /// </summary>
        /// <param name="etype"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IHasUnit MS_BasePeakIntensity(
            this IParamEdit etype, double value)
        {
            return etype.SetCvParam("MS:1000505").SetValue(value);
        }

        /// <summary>
        /// M/z value of the signal of highest intensity in the mass spectrum. [PSI:MS]
        /// </summary>
        /// <param name="etype"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IHasUnit MS_BasePeakMz(
            this IParamEdit etype, double value)
        {
            return etype.SetCvParam("MS:1000504").SetValue(value);
        }

        /// <summary>
        /// The sum of all the separate ion currents carried by the ions of different m/z contributing 
        /// to a complete mass spectrum or in a specified m/z range of a mass spectrum. [PSI:MS]
        /// </summary>
        /// <param name="etype"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IParamEdit MS_TotalIonCurrent(
            this IParamEdit etype, double value)
        {
            return etype.SetCvParam("MS:1000285").SetValue(value).NoUnit();
        }

        #endregion

        #region binary data

        /// <summary>
        /// A data array of m/z values. [PSI:MS]
        /// </summary>
        /// <param name="etype"></param>
        /// <returns></returns>
        public static IParamEdit MS_MzArray(
            this IParamEdit etype)
        {
            return etype.SetCvParam("MS:1000514").NoValue().MS_Mz();
        }

        public static bool Is_MS_MzArray(
            this IParamEdit etype)
        {
            return etype.HasCvParam("MS:1000514");
        }

        /// <summary>
        /// A data array of intensity values. [PSI:MS]
        /// </summary>
        /// <param name="etype"></param>
        /// <returns></returns>
        public static IHasUnit MS_IntensityArray(
            this IParamEdit etype)
        {
            return etype.SetCvParam("MS:1000515").NoValue();
        }

        public static bool Is_MS_IntensityArray(
            this IParamEdit etype)
        {
            return etype.HasCvParam("MS:1000515");
        }

        /// <summary>
        /// A data array of relative time offset values from a reference time. [PSI:MS]
        /// </summary>
        /// <param name="etype"></param>
        /// <returns></returns>
        public static IHasUnit MS_TimeArray(
            this IParamEdit etype)
        {
            return etype.SetCvParam("MS:1000595").NoValue();
        }

        public static bool Is_MS_TimeArray(
            this IParamEdit etype)
        {
            return etype.HasCvParam("MS:1000595");
        }

        /// <summary>
        /// No Compression. [PSI:MS]
        /// </summary>
        /// <param name="etype"></param>
        /// <returns></returns>
        public static IParamEdit MS_NoCompression(
            this IParamEdit etype)
        {
            return etype.SetCvParam("MS:1000576").NoValue().NoUnit();
        }

        public static bool Is_MS_NoCompression(
            this IParamEdit etype)
        {
            return etype.HasCvParam("MS:1000576");
        }

        /// <summary>
        /// Zlib (gzip) Compression. [PSI:MS]
        /// </summary>
        /// <param name="etype"></param>
        /// <returns></returns>
        public static IParamEdit MS_ZlibCompression(
            this IParamEdit etype)
        {
            return etype.SetCvParam("MS_1000574").NoValue().NoUnit();
        }

        public static bool Is_MS_ZlibCompression(
            this IParamEdit etype)
        {
            return etype.HasCvParam("MS_1000574");
        }

        /// <summary>
        /// Compression using MS-Numpress linear prediction compression. [https://github.com/fickludd/ms-numpress]
        /// </summary>
        /// <param name="etype"></param>
        /// <returns></returns>
        public static IParamEdit MS_MSNumpressLinearPredictionCompression(
            this IParamEdit etype)
        {
            return etype.SetCvParam("MS:1002312").NoValue().NoUnit();
        }

        /// <summary>
        /// Compression using MS-Numpress positive integer compression. [https://github.com/fickludd/ms-numpress]
        /// </summary>
        /// <param name="etype"></param>
        /// <returns></returns>
        public static IParamEdit MS_MSNumpressPositiveIntegerCompression(
            this IParamEdit etype)
        {
            return etype.SetCvParam("MS:1002313").NoValue().NoUnit();
        }

        /// <summary>
        /// Compression using MS-Numpress short logged float compression. [https://github.com/fickludd/ms-numpress]
        /// </summary>
        /// <param name="etype"></param>
        /// <returns></returns>
        public static IParamEdit MS_MSNumpressShortLoggedFloatCompression(
            this IParamEdit etype)
        {
            return etype.SetCvParam("MS:1002314").NoValue().NoUnit();
        }

        /// <summary>
        /// 64-bit precision little-endian floating point conforming to IEEE-754. [PSI:MS]
        /// </summary>
        /// <param name="etype"></param>
        /// <returns></returns>
        public static IParamEdit MS_64BitFloat(
            this IParamEdit etype)
        {
            return etype.SetCvParam("MS:1000523").NoValue().NoUnit();
        }

        public static bool Is_MS_64BitFloat(
            this IParamEdit etype)
        {
            return etype.HasCvParam("MS:1000523");
        }

        /// <summary>
        /// 32-bit precision little-endian floating point conforming to IEEE-754. [PSI:MS]
        /// </summary>
        /// <param name="etype"></param>
        /// <returns></returns>
        public static IParamEdit MS_32BitFloat(
            this IParamEdit etype)
        {
            return etype.SetCvParam("MS:1000521").NoValue().NoUnit();
        }

        public static bool Is_MS_32BitFloat(
            this IParamEdit etype)
        {
            return etype.HasCvParam("MS:1000521");
        }

        /// <summary>
        /// Signed 64-bit little-endian integer. [PSI:MS]
        /// </summary>
        /// <param name="etype"></param>
        /// <returns></returns>
        public static IParamEdit MS_64BitInteger(
            this IParamEdit etype)
        {
            return etype.SetCvParam("MS:1000522").NoValue().NoUnit();
        }

        public static bool Is_MS_64BitInteger(
            this IParamEdit etype)
        {
            return etype.HasCvParam("MS:1000522");
        }

        /// <summary>
        /// Signed 32-bit little-endian integer. [PSI:MS]
        /// </summary>
        /// <param name="etype"></param>
        /// <returns></returns>
        public static IParamEdit MS_32BitInteger(
            this IParamEdit etype)
        {
            return etype.SetCvParam("MS:1000519").NoValue().NoUnit();
        }

        public static bool Is_MS_32BitInteger(
            this IParamEdit etype)
        {
            return etype.HasCvParam("MS:1000519");
        }

        #endregion

        #region ion selection

        /// <summary>
        /// The primary or reference m/z about which the isolation window is defined. [PSI:MS]
        /// </summary>
        /// <param name="etype"></param>
        /// <param name="mz"></param>
        /// <returns></returns>
        public static IParamEdit MS_IsolationWindowTargetMz(
            this IParamEdit etype, double mz)
        {
            if (mz < 0)
                throw new ArgumentOutOfRangeException("mz");
            return etype.SetCvParam("MS:1000827").SetValue(mz).MS_Mz();
        }

        /// <summary>
        /// The primary or reference m/z about which the isolation window is defined. [PSI:MS]
        /// </summary>
        public static IValueConverter Get_MS_IsolationWindowTargetMz(
            this IParamEdit etype)
        {
            return etype.GetCvParam("MS:1000827");
        }

        /// <summary>
        /// The extent of the isolation window in m/z below the isolation window target m/z. 
        /// The lower and upper offsets may be asymmetric about the target m/z. [PSI:MS]
        /// </summary>
        /// <param name="etype"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static IParamEdit MS_IsolationWindowLowerOffset(
            this IParamEdit etype, double offset)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            return etype.SetCvParam("MS:1000828").SetValue(offset).MS_Mz();
        }

        /// <summary>
        /// The extent of the isolation window in m/z below the isolation window target m/z. 
        /// The lower and upper offsets may be asymmetric about the target m/z. [PSI:MS]
        /// </summary>
        public static IValueConverter Get_MS_IsolationWindowLowerOffset(
            this IParamEdit etype)
        {
            return etype.GetCvParam("MS:1000828");
        }

        /// <summary>
        /// The extent of the isolation window in m/z above the isolation window target m/z.
        /// The lower and upper offsets may be asymmetric about the target m/z. [PSI:MS]
        /// </summary>
        /// <param name="etype"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static IParamEdit MS_IsolationWindowUpperOffset(
            this IParamEdit etype, double offset)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            return etype.SetCvParam("MS:1000829").SetValue(offset).MS_Mz();
        }

        /// <summary>
        /// The extent of the isolation window in m/z above the isolation window target m/z.
        /// The lower and upper offsets may be asymmetric about the target m/z. [PSI:MS]
        /// </summary>
        public static IValueConverter Get_MS_IsolationWindowUpperOffset(
            this IParamEdit etype)
        {
            return etype.GetCvParam("MS:1000829");
        }

        /// <summary>
        /// Mass-to-charge ratio of a precursor ion selected for fragmentation. [PSI:PI]
        /// </summary>
        /// <param name="etype"></param>
        /// <param name="mz"></param>
        /// <returns></returns>
        public static IParamEdit MS_SelectedIonMz(
            this IParamEdit etype, double mz)
        {
            if (mz < 0)
                throw new ArgumentOutOfRangeException("mz");
            return etype.SetCvParam("MS:1002234").SetValue(mz).MS_Mz();
        }

        /// <summary>
        /// Mass-to-charge ratio of a precursor ion selected for fragmentation. [PSI:PI]
        /// </summary>
        public static IValueConverter Get_MS_SelectedIonMz(
            this IParamEdit etype)
        {            
            return etype.GetCvParam("MS:1002234");
        }

        /// <summary>
        /// The charge state of the ion, single or multiple and positive or negatively charged. [PSI:MS]
        /// </summary>
        /// <param name="etype"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IParamEdit MS_ChargeState(
            this IParamEdit etype, int state)
        {
            return etype.SetCvParam("MS:1000041").SetValue(state).NoUnit();
        }
                

        /// <summary>
        /// Energy for an ion experiencing collision with a stationary gas particle resulting in dissociation of the ion. [PSI:MS]
        /// </summary>
        /// <param name="etype"></param>
        /// <param name="ce"></param>
        /// <returns></returns>
        public static IParamEdit MS_CollisionEnergy(
            this IParamEdit etype, double ce)
        {
            if (ce < 0)
                throw new ArgumentOutOfRangeException("ce");
            return etype.SetCvParam("MS:1000045").SetValue(ce).UO_Electronvolt();
        }

        #endregion

        #region scan

        /// <summary>
        /// The time that an analyzer started a scan, relative to the start of the MS run. [PSI:MS]
        /// </summary>
        /// <param name="etype"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IHasUnit MS_ScanStartTime(
            this IParamEdit etype, double value)
        {
            return etype.SetCvParam("MS:1000016").SetValue(value);
        }

        /// <summary>
        /// The time that an analyzer started a scan, relative to the start of the MS run. [PSI:MS]
        /// </summary>
        public static IValueConverter Get_MS_ScanStartTime(this IParamEdit etype)
        {
            return etype.GetCvParam("MS:1000016");
        }

        /// <summary>
        /// A string unique to Thermo instrument describing instrument settings for the scan. [PSI:MS]
        /// </summary>        
        public static IParamEdit MS_FilterString(this IParamEdit etype, string filterString)
        {
            if (string.IsNullOrWhiteSpace(filterString))
                filterString = string.Empty;
            return etype.SetCvParam("MS:1000512").SetValue(filterString).NoUnit();
        }

        /// <summary>
        /// A string unique to Thermo instrument describing instrument settings for the scan. [PSI:MS]
        /// </summary>
        public static IValueConverter Get_MS_FilterString(this IParamEdit etype)
        {
            return etype.GetCvParam("MS:1000512");
        }

        // peak intensity        

        // collision-induced dissociation

        #endregion

        #region units

        public static IParamEdit MS_Mz(this IHasUnit attr)
        {
            return attr.SetUnit("MS:1000040");
        }

        public static IParamEdit MS_NumberOfCounts(this IHasUnit attr)
        {
            return attr.SetUnit("MS:1000131");
        }

        #endregion

        #region identification        

        /// <summary>
        /// The theoretical mass of the molecule (e.g. the peptide sequence and its modifications). [MS:PI]
        /// </summary>        
        /// <param name="etype"></param>
        /// <param name="mass"></param>
        /// <returns></returns>
        public static IParamEdit MS_TheoreticalMass(
            this IParamEdit etype, double mass)
        {
            return etype.SetCvParam("MS:1001117").SetValue(mass).UO_Dalton();
        }

        /// <summary>
        /// Andromeda is a peptide search engine. [MS:PI]
        /// </summary>        
        /// <param name="etype"></param>
        /// <returns></returns>
        public static IParamEdit MS_Andromeda(
            this IParamEdit etype)
        {
            return etype.SetCvParam("MS:1002337").NoValue().NoUnit();
        }

        /// <summary>
        /// The probability based score of the Andromeda search engine. [MS:PI]
        /// </summary>        
        /// <param name="etype"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public static IParamEdit MS_AndromedaScore(
            this IParamEdit etype, double score)
        {
            return etype.SetCvParam("MS:1002338").SetValue(score).NoUnit();
        }

        /// <summary>
        /// MaxQuant is a quantitative proteomics software package designed for analyzing large mass spectrometric data sets. 
        /// It is specifically aimed at high resolution MS data. [MS:MS]
        /// </summary>        
        /// <param name="etype"></param>
        /// <returns></returns>
        public static IParamEdit MS_MaxQuant(
            this IParamEdit etype)
        {
            return etype.SetCvParam("MS:1001583").NoValue().NoUnit();
        }

        /// <summary>
        /// The data type PEP (posterior error probability) produced by MaxQuant. [MS:MS]
        /// </summary>        
        /// <param name="etype"></param>
        /// <param name="pep"></param>
        /// <returns></returns>
        public static IParamEdit MS_MaxQuantPEP(
            this IParamEdit etype, double pep)
        {
            return etype.SetCvParam("MS:1001901").SetValue(pep).NoUnit();
        }

        /// <summary>
        /// The data type feature intensity produced by MaxQuant. [MS:MS]
        /// </summary>        
        /// <param name="etype"></param>
        /// <param name="pep"></param>
        /// <returns></returns>
        public static IParamEdit MS_MaxQuantFeatureIntensity(
            this IParamEdit etype, double intensity)
        {
            return etype.SetCvParam("MS:1001903").SetValue(intensity).NoUnit();
        }

        #endregion
    }
}
