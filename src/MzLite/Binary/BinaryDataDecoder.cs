#region license
// The MIT License (MIT)

// BinaryDataDecoder.cs

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

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using MzLite.Commons.Arrays;

namespace MzLite.Binary
{
    public class BinaryDataDecoder
    {

        public BinaryDataDecoder() { }
        
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
                int len = reader.ReadInt32();
                Peak1D[] peaks = new Peak1D[len];

                for (int i = 0; i < len; i++)
                {
                    double intensity = ReadValue(reader, peakArray.IntensityDataType);
                    double mz = ReadValue(reader, peakArray.MzDataType);
                    peaks[i] = new Peak1D(intensity, mz);
                }

                peakArray.Peaks = peaks.ToMzLiteArray();
            }
        }

        private static void NoCompression(Stream stream, Peak2DArray peakArray)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {

                int len = reader.ReadInt32();
                Peak2D[] peaks = new Peak2D[len];

                for (int i = 0; i < len; i++)
                {
                    double intensity = ReadValue(reader, peakArray.IntensityDataType);
                    double mz = ReadValue(reader, peakArray.MzDataType);
                    double rt = ReadValue(reader, peakArray.RtDataType);
                    peaks[i] = new Peak2D(intensity, mz, rt);
                }

                peakArray.Peaks = peaks.ToMzLiteArray();
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
