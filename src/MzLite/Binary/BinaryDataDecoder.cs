using System;
using System.IO;
using System.IO.Compression;
using MzLite.Model;
using System.Text;

namespace MzLite.Binary
{
    public class BinaryDataDecoder
    {
        
        public BinaryDataDecoder() {}

        public IPeakEnumerable Decode(PeakArray peakArray, byte[] bytes)
        {
            using (var memoryStream = new MemoryStream(bytes))
            {
                return Decode(memoryStream, peakArray);
            }
        }

        public virtual IPeakEnumerable Decode(Stream stream, PeakArray peakArray)
        {            

            switch (peakArray.PeakType)
            {
                case PeakType.Peak1D:
                    IPeak1D[] peaks1d = new IPeak1D[peakArray.ArrayLength];
                    Decode1D(stream, peakArray.AsPeakArray1D, peaks1d);
                    return new PeakArrayEnumerable(peaks1d);                 
                case PeakType.Peak2D:
                    IPeak2D[] peaks2d = new IPeak2D[peakArray.ArrayLength];
                    Decode2D(stream, peakArray.AsPeakArray2D, peaks2d);
                    return new PeakArrayEnumerable(peaks2d);                   
                default:
                    throw new NotSupportedException("Peak type not supported: " + peakArray.PeakType.ToString());
            }
        }

        private static void Decode1D(Stream stream, Peak1DArray peakArray, IPeak1D[] peaks1d)
        {
            switch (peakArray.CompressionType)
            {
                case BinaryDataCompressionType.NoCompression:
                    NoCompression(stream, peakArray, peaks1d);
                    break;
                case BinaryDataCompressionType.ZLib:
                    ZLib(stream, peakArray, peaks1d);
                    break;
                default:
                    throw new NotSupportedException("Compression type not supported: " + peakArray.CompressionType.ToString());
            }
        }

        private static void Decode2D(Stream stream, Peak2DArray peakArray, IPeak2D[] peaks2d)
        {
            switch (peakArray.CompressionType)
            {
                case BinaryDataCompressionType.NoCompression:
                    NoCompression(stream, peakArray, peaks2d);
                    break;
                case BinaryDataCompressionType.ZLib:
                    ZLib(stream, peakArray, peaks2d);
                    break;
                default:
                    throw new NotSupportedException("Compression type not supported: " + peakArray.CompressionType.ToString());
            }
        }

        private static void NoCompression(Stream stream, Peak1DArray peakArray, IPeak1D[] peaks1d)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                for (int i = 0; i < peakArray.ArrayLength; i++)
                {
                    double intensity = ReadValue(reader, peakArray.IntensityDataType);
                    double mz = ReadValue(reader, peakArray.MzDataType);
                    peaks1d[i] = new Peak1D(intensity, mz);
                }
            }
        }

        private static void NoCompression(Stream stream, Peak2DArray peakArray, IPeak2D[] peaks2d)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                for (int i = 0; i < peakArray.ArrayLength; i++)
                {
                    double intensity = ReadValue(reader, peakArray.IntensityDataType);
                    double mz = ReadValue(reader, peakArray.MzDataType);
                    double rt = ReadValue(reader, peakArray.RtDataType);
                    peaks2d[i] = new Peak2D(intensity, mz, rt);
                }
            }
        }

        private static void ZLib(Stream stream, Peak1DArray peakArray, IPeak1D[] peaks1d)
        {
            using (var decompressStream = new DeflateStream(stream, CompressionMode.Decompress,true))            
            {                
                NoCompression(decompressStream, peakArray,peaks1d);
            }
        }

        private static void ZLib(Stream stream, Peak2DArray peakArray, IPeak2D[] peaks2d)
        {
            using (var decompressStream = new DeflateStream(stream, CompressionMode.Decompress,true))
            {
                NoCompression(decompressStream, peakArray,peaks2d);
            }
        }

        private static double ReadValue(BinaryReader reader, BinaryDataType binaryDataType)
        {
            switch (binaryDataType)
            {
                case BinaryDataType.FLoat32:
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
