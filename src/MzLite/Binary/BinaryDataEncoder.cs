using System.Collections.Generic;
using MzLite.Model;
using System.Linq;
using System;
using System.IO;
using System.IO.Compression;

namespace MzLite.Binary
{
    public class BinaryDataEncoder : IDisposable
    {

        public readonly static int InitialBufferSize = 1048576;

        private readonly MemoryStream memoryStream;

        public BinaryDataEncoder() : this(InitialBufferSize) { }

        public BinaryDataEncoder(int initialBufferSize)
        {
            memoryStream = new MemoryStream(initialBufferSize);
        }

        public byte[] Encode(PeakArray peakArray, IPeakEnumerable peaks)
        {
            peakArray.ArrayLength = peaks.ArrayLength;
            return Encode(memoryStream, peakArray, peaks);
        }

        private static byte[] Encode(MemoryStream memoryStream, PeakArray peakArray, IEnumerable<IPeak> peaks)
        {

            memoryStream.Position = 0;

            switch (peakArray.PeakType)
            {
                case PeakType.Peak1D:
                    Encode1D(memoryStream, peakArray.AsPeakArray1D, peaks.Select(x => x.AsPeak1D));
                    break;
                case PeakType.Peak2D:
                    Encode2D(memoryStream, peakArray.AsPeakArray2D, peaks.Select(x => x.AsPeak2D));
                    break;
                default:
                    throw new NotSupportedException("Peak type not supported: " + peakArray.PeakType.ToString());
            }            

            memoryStream.Position = 0;
            return memoryStream.ToArray();
        }

        private static void Encode1D(MemoryStream memoryStream, Peak1DArray peakArray, IEnumerable<IPeak1D> peaks)
        {
            switch (peakArray.CompressionType)
            {
                case BinaryDataCompressionType.NoCompression:
                    NoCompression(memoryStream, peakArray, peaks);
                    break;
                case BinaryDataCompressionType.ZLib:
                    ZLib(memoryStream, peakArray, peaks);
                    break;
                default:
                    throw new NotSupportedException("Compression type not supported: " + peakArray.CompressionType.ToString());
            }
        }

        private static void Encode2D(MemoryStream memoryStream, Peak2DArray peakArray, IEnumerable<IPeak2D> peaks)
        {
            switch (peakArray.CompressionType)
            {
                case BinaryDataCompressionType.NoCompression:
                    NoCompression(memoryStream, peakArray, peaks);
                    break;
                case BinaryDataCompressionType.ZLib:
                    ZLib(memoryStream, peakArray, peaks);
                    break;
                default:
                    throw new NotSupportedException("Compression type not supported: " + peakArray.CompressionType.ToString());
            }
        }

        private static void NoCompression(Stream memoryStream, Peak1DArray peakArray, IEnumerable<IPeak1D> peaks)
        {
            using (var writer = new BinaryWriter(memoryStream))
            {
                foreach (var pk in peaks)
                {
                    WriteValue(writer, peakArray.IntensityDataType, pk.Intensity);
                    WriteValue(writer, peakArray.MzDataType, pk.Mz);
                }
            }

        }

        private static void NoCompression(Stream memoryStream, Peak2DArray peakArray, IEnumerable<IPeak2D> peaks)
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
        }

        private static void ZLib(Stream memoryStream, Peak1DArray peakArray, IEnumerable<IPeak1D> peaks)
        {
            using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress, true))
            {
                NoCompression(deflateStream, peakArray, peaks);
            }
        }

        private static void ZLib(Stream memoryStream, Peak2DArray peakArray, IEnumerable<IPeak2D> peaks)
        {
            using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress, true))
            {
                NoCompression(deflateStream, peakArray, peaks);
            }
        }

        private static void WriteValue(BinaryWriter writer, BinaryDataType binaryDataType, double value)
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

        #region IDisposable Members

        private bool isDisposed = false;

        public void Dispose()
        {
            if (isDisposed)
                return;

            memoryStream.Dispose();

            isDisposed = true;
        }

        #endregion
    }


}
