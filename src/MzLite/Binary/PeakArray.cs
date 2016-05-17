﻿using MzLite.Model;
using Newtonsoft.Json;

namespace MzLite.Binary
{

    public interface IPeak
    {
        double Intensity { get; }
    }
    
    public struct Peak1D : IPeak
    {

        private readonly double intensity;
        private readonly double mz;

        public Peak1D(double intensity, double mz)
            : this()
        {
            this.intensity = intensity;
            this.mz = mz;
        }

        public double Intensity
        {
            get { return intensity; }
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

    public struct Peak2D : IPeak
    {

        private readonly double intensity;
        private readonly double mz;
        private readonly double rt;

        public Peak2D(double intensity, double mz, double rt)
            : this()
        {
            this.intensity = intensity;
            this.mz = mz;
            this.rt = rt;
        }


        public double Intensity
        {
            get { return intensity; }
        }

        public double Mz
        {
            get { return mz; }
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

    public abstract class PeakArray<TPeak> : ParamContainer
        where TPeak : IPeak
    {

        private readonly TPeak[] peaks;
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
            this.peaks = new TPeak[arrayLength];
        }

        [JsonProperty(Required = Required.Always)]
        public BinaryDataType IntensityDataType { get { return intensityDataType; } }

        [JsonProperty(Required = Required.Always)]
        public BinaryDataCompressionType CompressionType { get { return compressionType; } }

        [JsonProperty(Required = Required.Always)]
        public int ArrayLength { get { return arrayLength; } }

        public TPeak[] Peaks { get { return peaks; } }

    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Peak1DArray : PeakArray<Peak1D>
    {

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
        }

        [JsonProperty(Required = Required.Always)]
        public BinaryDataType MzDataType { get { return mzDataType; } }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Peak2DArray : PeakArray<Peak2D>
    {

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
        }

        [JsonProperty(Required = Required.Always)]
        public BinaryDataType MzDataType { get { return mzDataType; } }

        [JsonProperty(Required = Required.Always)]
        public BinaryDataType RtDataType { get { return rtDataType; } }

    }
}
