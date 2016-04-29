using Newtonsoft.Json;

namespace MzLite.Model
{
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

    public interface IPeak
    {
        double Intensity { get; }
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

        public override string ToString()
        {
            return string.Format("intensity={0}, mz={1}", Intensity, Mz);
        }

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

        public override string ToString()
        {
            return string.Format("intensity={0}, mz={1}, rt={2}", Intensity, Mz, Rt);
        }
    }

    public abstract class PeakArray : ParamContainer
    {

        internal PeakArray()
        {
            this.CompressionType = BinaryDataCompressionType.NoCompression;
            this.IntensityDataType = BinaryDataType.Float64;
            this.ArrayLength = 0;
        }

        [JsonProperty(Required = Required.Always)]
        public BinaryDataType IntensityDataType { get; set; }

        [JsonProperty(Required = Required.Always)]
        public BinaryDataCompressionType CompressionType { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int ArrayLength { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Peak1DArray : PeakArray
    {

        public Peak1DArray()
            : base()
        {
            this.MzDataType = BinaryDataType.Float64;
        }

        [JsonProperty(Required = Required.Always)]
        public BinaryDataType MzDataType { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Peak2DArray : Peak1DArray
    {

        public Peak2DArray()
            : base()
        {
            this.RtDataType = BinaryDataType.Float64;
        }

        [JsonProperty(Required = Required.Always)]
        public BinaryDataType RtDataType { get; set; }

    }
}
