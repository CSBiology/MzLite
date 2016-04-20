using System.Collections.Generic;
using MzLite.Model;
using System.Linq;
using System;
using System.IO;
using System.IO.Compression;

namespace MzLite.Binary
{
    public class BinaryDataEncoder
    {

        public readonly static int InitialBufferSize = 1048576;

        public virtual byte[] Encode(PeakArray peakArray, IPeakEnumerable peaks)
        {

            byte[] bytes;

            switch (peakArray.PeakType)
            {
                case PeakType.Peak1D:
                    bytes = ToBytes(peakArray.AsPeakArray1D, peaks.Select(x => x.AsPeak1D));
                    break;
                case PeakType.Peak2D:
                    bytes = ToBytes(peakArray.AsPeakArray2D, peaks.Select(x => x.AsPeak2D));
                    break;
                default:
                    throw new NotSupportedException("Peak type not supported: " + peakArray.PeakType.ToString());
            }

            peakArray.ArrayLength = peaks.ArrayLength;

            return bytes;
        }

        private byte[] ToBytes(Peak1DArray peakArray, IEnumerable<IPeak1D> peaks)
        {
            switch (peakArray.CompressionType)
            {
                case BinaryDataCompressionType.NoCompression:
                    return NoCompression(peakArray, peaks);
                case BinaryDataCompressionType.ZLib:
                    return ZLib(peakArray, peaks);
                default:
                    throw new NotSupportedException("Compression type not supported: " + peakArray.CompressionType.ToString());
            }
        }

        private byte[] ToBytes(Peak2DArray peakArray, IEnumerable<IPeak2D> peaks)
        {
            switch (peakArray.CompressionType)
            {
                case BinaryDataCompressionType.NoCompression:
                    return NoCompression(peakArray, peaks);
                case BinaryDataCompressionType.ZLib:
                    return ZLib(peakArray, peaks);
                default:
                    throw new NotSupportedException("Compression type not supported: " + peakArray.CompressionType.ToString());
            }
        }

        private byte[] NoCompression(Peak1DArray peakArray, IEnumerable<IPeak1D> peaks)
        {
            using (var memoryStream = new MemoryStream(InitialBufferSize))
            {
                using (var writer = new BinaryWriter(memoryStream))
                {
                    foreach (var pk in peaks)
                    {
                        WriteValue(writer, peakArray.IntensityDataType, pk.Intensity);
                        WriteValue(writer, peakArray.MzDataType, pk.Mz);
                    }
                }

                memoryStream.Position = 0;
                return memoryStream.ToArray();
            }
        }

        private byte[] NoCompression(Peak2DArray peakArray, IEnumerable<IPeak2D> peaks)
        {
            using (var memoryStream = new MemoryStream(InitialBufferSize))
            {
                using (var writer = new BinaryWriter(memoryStream))
                {
                    foreach (var pk in peaks)
                    {
                        WriteValue(writer, peakArray.IntensityDataType, pk.Intensity);
                        WriteValue(writer, peakArray.MzDataType, pk.Mz);
                        WriteValue(writer, peakArray.RtDataType, pk.Rt);
                    }
                }

                memoryStream.Position = 0;
                return memoryStream.ToArray();
            }
        }

        private byte[] ZLib(Peak1DArray peakArray, IEnumerable<IPeak1D> peaks)
        {
            using (var memoryStream = new MemoryStream(InitialBufferSize))
            {
                using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress, true))
                using (var writer = new BinaryWriter(deflateStream))
                {
                    foreach (var pk in peaks)
                    {
                        WriteValue(writer, peakArray.IntensityDataType, pk.Intensity);
                        WriteValue(writer, peakArray.MzDataType, pk.Mz);
                    }
                }

                memoryStream.Position = 0;
                return memoryStream.ToArray();
            }
        }

        private byte[] ZLib(Peak2DArray peakArray, IEnumerable<IPeak2D> peaks)
        {
            using (var memoryStream = new MemoryStream(InitialBufferSize))
            {
                using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress, true))
                using (var writer = new BinaryWriter(deflateStream))
                {
                    foreach (var pk in peaks)
                    {
                        WriteValue(writer, peakArray.IntensityDataType, pk.Intensity);
                        WriteValue(writer, peakArray.MzDataType, pk.Mz);
                        WriteValue(writer, peakArray.RtDataType, pk.Rt);
                    }
                }

                memoryStream.Position = 0;
                return memoryStream.ToArray();
            }
        }

        private void WriteValue(BinaryWriter writer, BinaryDataType binaryDataType, double value)
        {
            switch (binaryDataType)
            {
                case BinaryDataType.FLoat32:
                    writer.Write((float)value);
                    break;
                case BinaryDataType.Float64:
                    writer.Write(value);
                    break;
                case BinaryDataType.Int32:
                    writer.Write((int)value);
                    break;
                case BinaryDataType.Int64:
                    writer.Write((long)value);
                    break;
                default:
                    throw new NotSupportedException("Data type not supported: " + binaryDataType.ToString());
            }
        }
    }


}
