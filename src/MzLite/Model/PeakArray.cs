using System;
using Newtonsoft.Json;

namespace MzLite.Model
{
    public enum PeakType
    {
        Peak1D = 0, Peak2D = 1
    }

    public enum BinaryDataType
    {
        FLoat32 = 0,
        Float64 = 1,
        Int32 = 2,
        Int64 = 3
    }

    public enum BinaryDataCompressionType
    {
        NoCompression = 0,
        ZLib = 1
    }

    public interface IPeak
    {
        double Intensity { get; }
        PeakType PeakType { get; }
        IPeak1D AsPeak1D { get; }
        IPeak2D AsPeak2D { get; }
    }

    public interface IPeak1D : IPeak
    {
        double Mz { get; }
    }

    public interface IPeak2D : IPeak1D
    {
        double Rt { get; }
    }

    public struct Peak1D : IPeak1D
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

        public PeakType PeakType { get { return PeakType.Peak1D; } }
        public IPeak1D AsPeak1D { get { return this; } }
        public IPeak2D AsPeak2D { get { throw new InvalidCastException(); } }
    }

    public struct Peak2D : IPeak2D
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

        public PeakType PeakType { get { return PeakType.Peak2D; } }
        public IPeak1D AsPeak1D { get { return this; } }
        public IPeak2D AsPeak2D { get { return this; } }
    }

    public abstract class PeakArray : ParamContainer
    {

        [JsonProperty("PeakType", Required = Required.Always)]
        private readonly PeakType peakType;

        internal PeakArray(PeakType peakType)
        {
            this.peakType = peakType;
            this.CompressionType = BinaryDataCompressionType.NoCompression;
            this.IntensityDataType = BinaryDataType.Float64;
            this.ArrayLength = 0;
        }

        public PeakType PeakType { get { return peakType; } }

        [JsonProperty(Required = Required.Always)]
        public BinaryDataType IntensityDataType { get; set; }

        [JsonProperty(Required = Required.Always)]
        public BinaryDataCompressionType CompressionType { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int ArrayLength { get; set; }

        public abstract Peak1DArray AsPeakArray1D { get; }
        public abstract Peak2DArray AsPeakArray2D { get; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Peak1DArray : PeakArray
    {

        internal Peak1DArray(PeakType peakType)
            : base(peakType)
        {
            this.MzDataType = BinaryDataType.Float64;
        }

        public Peak1DArray()
            : base(PeakType.Peak1D)
        {
            this.MzDataType = BinaryDataType.Float64;
        }

        [JsonProperty(Required = Required.Always)]
        public BinaryDataType MzDataType { get; set; }

        public override Peak1DArray AsPeakArray1D { get { return this; } }
        public override Peak2DArray AsPeakArray2D { get { throw new InvalidCastException(); } }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Peak2DArray : Peak1DArray
    {
        public Peak2DArray()
            : base(PeakType.Peak2D)
        {
            this.RtDataType = BinaryDataType.Float64;
        }

        [JsonProperty(Required = Required.Always)]
        public BinaryDataType RtDataType { get; set; }

        public override Peak2DArray AsPeakArray2D { get { return this; } }
    }
}
