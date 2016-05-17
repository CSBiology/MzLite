using System;
using System.IO;
using System.IO.Compression;
using System.Text;

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

        public byte[] Encode(Peak1DArray peakArray)
        {
            memoryStream.Position = 0;
            switch (peakArray.CompressionType)
            {
                case BinaryDataCompressionType.NoCompression:
                    NoCompression(memoryStream, peakArray);
                    break;
                case BinaryDataCompressionType.ZLib:
                    ZLib(memoryStream, peakArray);
                    break;
                default:
                    throw new NotSupportedException("Compression type not supported: " + peakArray.CompressionType.ToString());
            }
            memoryStream.Position = 0;
            return memoryStream.ToArray();
        }

        public byte[] Encode(Peak2DArray peakArray)
        {            
            memoryStream.Position = 0;
            switch (peakArray.CompressionType)
            {
                case BinaryDataCompressionType.NoCompression:
                    NoCompression(memoryStream, peakArray);
                    break;
                case BinaryDataCompressionType.ZLib:
                    ZLib(memoryStream, peakArray);
                    break;
                default:
                    throw new NotSupportedException("Compression type not supported: " + peakArray.CompressionType.ToString());
            }
            memoryStream.Position = 0;
            return memoryStream.ToArray();
        }                

        private static void NoCompression(Stream memoryStream, Peak1DArray peakArray)
        {
            using (var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true))
            {
                foreach (var pk in peakArray.Peaks)
                {
                    WriteValue(writer, peakArray.IntensityDataType, pk.Intensity);
                    WriteValue(writer, peakArray.MzDataType, pk.Mz);
                }
            }

        }

        private static void NoCompression(Stream memoryStream, Peak2DArray peakArray)
        {
            using (var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true))
            {
                foreach (var pk in peakArray.Peaks)
                {
                    WriteValue(writer, peakArray.IntensityDataType, pk.Intensity);
                    WriteValue(writer, peakArray.MzDataType, pk.Mz);
                    WriteValue(writer, peakArray.RtDataType, pk.Rt);
                }
            }
        }

        private static void ZLib(Stream memoryStream, Peak1DArray peakArray)
        {
            using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress, true))
            {
                NoCompression(deflateStream, peakArray);
            }
        }

        private static void ZLib(Stream memoryStream, Peak2DArray peakArray)
        {
            using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress, true))
            {
                NoCompression(deflateStream, peakArray);
            }
        }

        private static void WriteValue(BinaryWriter writer, BinaryDataType binaryDataType, double value)
        {
            switch (binaryDataType)
            {
                case BinaryDataType.Float32:
                    writer.Write(decimal.ToSingle(new decimal(value)));
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
