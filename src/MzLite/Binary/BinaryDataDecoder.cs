using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace MzLite.Binary
{
    public class BinaryDataDecoder
    {

        public BinaryDataDecoder() { }

        public void Decode(Peak1DArray peakArray, byte[] bytes)
        {
            using (var memoryStream = new MemoryStream(bytes))
            {
                Decode(memoryStream, peakArray);
            }
        }

        public void Decode(Stream stream, Peak1DArray peakArray)
        {

            switch (peakArray.CompressionType)
            {
                case BinaryDataCompressionType.NoCompression:
                    NoCompression(stream, peakArray);
                    break;
                case BinaryDataCompressionType.ZLib:
                    ZLib(stream, peakArray);
                    break;
                default:
                    throw new NotSupportedException("Compression type not supported: " + peakArray.CompressionType.ToString());
            }
        }

        public void Decode(Peak2DArray peakArray, byte[] bytes)
        {
            using (var memoryStream = new MemoryStream(bytes))
            {
                Decode(memoryStream, peakArray);
            }
        }

        public void Decode(Stream stream, Peak2DArray peakArray)
        {
            switch (peakArray.CompressionType)
            {
                case BinaryDataCompressionType.NoCompression:
                    NoCompression(stream, peakArray);
                    break;
                case BinaryDataCompressionType.ZLib:
                    ZLib(stream, peakArray);
                    break;
                default:
                    throw new NotSupportedException("Compression type not supported: " + peakArray.CompressionType.ToString());
            }
        }

        private static void NoCompression(Stream stream, Peak1DArray peakArray)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                for (int i = 0; i < peakArray.ArrayLength; i++)
                {
                    double intensity = ReadValue(reader, peakArray.IntensityDataType);
                    double mz = ReadValue(reader, peakArray.MzDataType);
                    peakArray.Peaks[i] = new Peak1D(intensity, mz);
                }
            }
        }

        private static void NoCompression(Stream stream, Peak2DArray peakArray)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                for (int i = 0; i < peakArray.ArrayLength; i++)
                {
                    double intensity = ReadValue(reader, peakArray.IntensityDataType);
                    double mz = ReadValue(reader, peakArray.MzDataType);
                    double rt = ReadValue(reader, peakArray.RtDataType);
                    peakArray.Peaks[i] = new Peak2D(intensity, mz, rt);
                }
            }
        }

        private static void ZLib(Stream stream, Peak1DArray peakArray)
        {
            using (var decompressStream = new DeflateStream(stream, CompressionMode.Decompress, true))
            {
                NoCompression(decompressStream, peakArray);
            }
        }

        private static void ZLib(Stream stream, Peak2DArray peakArray)
        {
            using (var decompressStream = new DeflateStream(stream, CompressionMode.Decompress, true))
            {
                NoCompression(decompressStream, peakArray);
            }
        }

        private static double ReadValue(BinaryReader reader, BinaryDataType binaryDataType)
        {
            switch (binaryDataType)
            {
                case BinaryDataType.Float32:
                    return decimal.ToDouble(new decimal(reader.ReadSingle()));
                case BinaryDataType.Float64:
                    return reader.ReadDouble();
                case BinaryDataType.Int32:
                    return (double)reader.ReadInt32();
                case BinaryDataType.Int64:
                    return (double)reader.ReadInt64();
                default:
                    throw new NotSupportedException("Data type not supported: " + binaryDataType.ToString());
            }
        }

    }
}
