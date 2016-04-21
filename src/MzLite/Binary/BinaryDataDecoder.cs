using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using MzLite.Model;
using System.Linq;

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

        public IPeakEnumerable Decode(Stream stream, PeakArray peakArray)
        {            
            switch (peakArray.PeakType)
            {
                case PeakType.Peak1D:
                    return new PeakArrayEnumerable(Decode1D(stream, peakArray.AsPeakArray1D).ToArray());                 
                case PeakType.Peak2D:
                    return new PeakArrayEnumerable(Decode2D(stream, peakArray.AsPeakArray2D).ToArray());                   
                default:
                    throw new NotSupportedException("Peak type not supported: " + peakArray.PeakType.ToString());
            }
        }

        private static IEnumerable<IPeak1D> Decode1D(Stream stream, Peak1DArray peakArray)
        {
            switch (peakArray.CompressionType)
            {
                case BinaryDataCompressionType.NoCompression:
                    return NoCompression(stream, peakArray);
                case BinaryDataCompressionType.ZLib:
                    return ZLib(stream, peakArray);
                default:
                    throw new NotSupportedException("Compression type not supported: " + peakArray.CompressionType.ToString());
            }
        }

        private static IEnumerable<IPeak2D> Decode2D(Stream stream, Peak2DArray peakArray)
        {
            switch (peakArray.CompressionType)
            {
                case BinaryDataCompressionType.NoCompression:
                    return NoCompression(stream, peakArray);
                case BinaryDataCompressionType.ZLib:
                    return ZLib(stream, peakArray);
                default:
                    throw new NotSupportedException("Compression type not supported: " + peakArray.CompressionType.ToString());
            }
        }

        private static IEnumerable<IPeak1D> NoCompression(Stream stream, Peak1DArray peakArray)
        {
            using (var reader = new BinaryReader(stream))
            {
                for (int i = 0; i < peakArray.ArrayLength; i++)
                {
                    double intensity = ReadValue(reader, peakArray.IntensityDataType);
                    double mz = ReadValue(reader, peakArray.MzDataType);
                    yield return new Peak1D(intensity, mz);
                }
            }
        }

        private static IEnumerable<IPeak2D> NoCompression(Stream stream, Peak2DArray peakArray)
        {
            using (var reader = new BinaryReader(stream))
            {
                for (int i = 0; i < peakArray.ArrayLength; i++)
                {
                    double intensity = ReadValue(reader, peakArray.IntensityDataType);
                    double mz = ReadValue(reader, peakArray.MzDataType);
                    double rt = ReadValue(reader, peakArray.RtDataType);
                    yield return new Peak2D(intensity, mz, rt);
                }
            }
        }

        private static IEnumerable<IPeak1D> ZLib(Stream stream, Peak1DArray peakArray)
        {
            using (var decompressStream = new DeflateStream(stream, CompressionMode.Decompress))
            {
                return NoCompression(decompressStream, peakArray);
            }
        }

        private static IEnumerable<IPeak2D> ZLib(Stream stream, Peak2DArray peakArray)
        {
            using (var decompressStream = new DeflateStream(stream, CompressionMode.Decompress))
            {
                return NoCompression(decompressStream, peakArray);
            }
        }

        private static double ReadValue(BinaryReader reader, BinaryDataType binaryDataType)
        {
            switch (binaryDataType)
            {
                case BinaryDataType.FLoat32:
                    return reader.ReadSingle();
                case BinaryDataType.Float64:
                    return reader.ReadDouble();
                case BinaryDataType.Int32:
                    return reader.ReadInt32();
                case BinaryDataType.Int64:
                    return reader.ReadInt64();
                default:
                    throw new NotSupportedException("Data type not supported: " + binaryDataType.ToString());
            }
        }
       
    }
}
