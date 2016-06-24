#region license
// The MIT License (MIT)

// PeakArray.cs

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

using MzLite.Model;
using Newtonsoft.Json;

namespace MzLite.Binary
{

    public abstract class Peak
    {

        private readonly double intensity;

        internal Peak(double intensity)
        {
            this.intensity = intensity;            
        }

        public double Intensity
        {
            get { return intensity; }
        }
    }

    public class Peak1D : Peak
    {
        
        private readonly double mz;

        public Peak1D(double intensity, double mz) 
            : base(intensity)
        {            
            this.mz = mz;
        }
        
        public double Mz
        {
            get { return mz; }
        }

        public override string ToString()
        {
            return string.Format("intensity={0}, mz={1}", Intensity, Mz);
        }

    }

    public class Peak2D : Peak1D
    {
        
        private readonly double rt;

        public Peak2D(double intensity, double mz, double rt) 
            : base(intensity, mz)
        {            
            this.rt = rt;
        }        

        public double Rt
        {
            get { return rt; }
        }

        public override string ToString()
        {
            return string.Format("intensity={0}, mz={1}, rt={2}", Intensity, Mz, Rt);
        }
    }

    public enum BinaryDataType
    {
        Float32 = 0,
        Float64 = 1,
        Int32 = 2,
        Int64 = 3
    }

    public enum BinaryDataCompressionType
    {
        NoCompression = 0,
        ZLib = 1
    }

    public abstract class PeakArray : ParamContainer        
    {


        private readonly int arrayLength;
        private readonly BinaryDataCompressionType compressionType;
        private readonly BinaryDataType intensityDataType;

        internal PeakArray(
            int arrayLength,
            BinaryDataCompressionType compressionType,
            BinaryDataType intensityDataType)
        {
            this.compressionType = compressionType;
            this.intensityDataType = intensityDataType;
            this.arrayLength = arrayLength;
        }

        [JsonProperty(Required = Required.Always)]
        public BinaryDataType IntensityDataType { get { return intensityDataType; } }

        [JsonProperty(Required = Required.Always)]
        public BinaryDataCompressionType CompressionType { get { return compressionType; } }

        [JsonProperty(Required = Required.Always)]
        public int ArrayLength { get { return arrayLength; } }

    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Peak1DArray : PeakArray
    {

        private readonly Peak1D[] peaks;
        private readonly BinaryDataType mzDataType;

        [JsonConstructor]
        public Peak1DArray(
            [JsonProperty("ArrayLength")] int arrayLength,
            [JsonProperty("CompressionType")] BinaryDataCompressionType compressionType,
            [JsonProperty("IntensityDataType")] BinaryDataType intensityDataType,
            [JsonProperty("MzDataType")] BinaryDataType mzDataType)
            : base(arrayLength, compressionType, intensityDataType)
        {
            this.mzDataType = mzDataType;
            this.peaks = new Peak1D[arrayLength];
        }

        [JsonProperty(Required = Required.Always)]
        public BinaryDataType MzDataType { get { return mzDataType; } }

        [JsonIgnore]
        public Peak1D[] Peaks { get { return peaks; } }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Peak2DArray : PeakArray
    {

        private readonly Peak2D[] peaks;
        private readonly BinaryDataType mzDataType;
        private readonly BinaryDataType rtDataType;

        [JsonConstructor]
        public Peak2DArray(
            [JsonProperty("ArrayLength")] int arrayLength,
            [JsonProperty("CompressionType")] BinaryDataCompressionType compressionType,
            [JsonProperty("IntensityDataType")] BinaryDataType intensityDataType,
            [JsonProperty("MzDataType")] BinaryDataType mzDataType,
            [JsonProperty("RtDataType")] BinaryDataType rtDataType)
            : base(arrayLength, compressionType, intensityDataType)
        {
            this.mzDataType = mzDataType;
            this.rtDataType = rtDataType;
            this.peaks = new Peak2D[arrayLength];
        }

        [JsonProperty(Required = Required.Always)]
        public BinaryDataType MzDataType { get { return mzDataType; } }

        [JsonProperty(Required = Required.Always)]
        public BinaryDataType RtDataType { get { return rtDataType; } }

        [JsonIgnore]
        public Peak2D[] Peaks { get { return peaks; } }
    }
}
