#region license
// The MIT License (MIT)

// Baf2SqlWrapper.cs

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
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace MzLite.Bruker
{

    /// <summary>
    /// Helper methods to use Bruker's BAF loading C-API DLL.
    /// </summary>
    public static class Baf2SqlWrapper
    {

        [DllImport("baf2sql_c", CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt32 baf2sql_get_sqlite_cache_filename
               (StringBuilder sql_filename_buf, UInt32 sql_filename_buflen, String baf_filename);

        [DllImport("baf2sql_c", CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt64 baf2sql_array_open_storage
               (int ignore_calibrator_ami, String filename);

        [DllImport("baf2sql_c", CallingConvention = CallingConvention.Cdecl)]
        public static extern void baf2sql_array_close_storage
               (UInt64 handle);

        [DllImport("baf2sql_c", CallingConvention = CallingConvention.Cdecl)]
        public static extern void baf2sql_array_get_num_elements
               (UInt64 handle, UInt64 id, ref UInt64 num_elements);

        [DllImport("baf2sql_c", CallingConvention = CallingConvention.Cdecl)]
        public static extern int baf2sql_array_read_double
               (UInt64 handle, UInt64 id, double[] buf);

        [DllImport("baf2sql_c", CallingConvention = CallingConvention.Cdecl)]
        public static extern int baf2sql_array_read_float
               (UInt64 handle, UInt64 id, float[] buf);

        [DllImport("baf2sql_c", CallingConvention = CallingConvention.Cdecl)]
        public static extern int baf2sql_array_read_uint32
               (UInt64 handle, UInt64 id, UInt32[] buf);

        [DllImport("baf2sql_c", CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt32 baf2sql_get_last_error_string
               (StringBuilder buf, UInt32 len);

        [DllImport("baf2sql_c", CallingConvention = CallingConvention.Cdecl)]
        public static extern void baf2sql_set_num_threads
               (UInt32 n);

        /// <summary>
        /// Throw last error string as an exception.
        /// </summary>
        public static void ThrowLastBaf2SqlError()
        {
            StringBuilder buf = new StringBuilder("");
            UInt32 len = baf2sql_get_last_error_string(buf, 0);
            buf.EnsureCapacity((int)(len + 1));
            baf2sql_get_last_error_string(buf, len);
            throw new Baf2SqlException(buf.ToString());
        }

        /// <summary>
        /// Find out the file name of the SQL cache corresponding to the specified BAF file.
        /// (If the SQL cache doesn't exist yet, it will be created.) */
        /// </summary>        
        public static String GetSQLiteCacheFilename(String baf_filename)
        {
            StringBuilder buf = new StringBuilder("");
            UInt32 len = baf2sql_get_sqlite_cache_filename(buf, 0, baf_filename);
            if (len == 0) ThrowLastBaf2SqlError();

            buf.EnsureCapacity((int)(len + 1));
            len = baf2sql_get_sqlite_cache_filename(buf, len, baf_filename);
            if (len == 0) ThrowLastBaf2SqlError();

            return buf.ToString();
        }

        /// <summary>
        /// Given the Id of one spectral component (e.g., a 'ProfileMzId' from the SQL cache),
        /// load the binary data from the BAF (returning a double array).
        /// </summary>        
        public static double[] GetBafDoubleArray(UInt64 handle, UInt64 id)
        {
            UInt64 n = 0;
            baf2sql_array_get_num_elements(handle, id, ref n);

            double[] myArray = new double[n];
            int rc = baf2sql_array_read_double(handle, id, myArray);
            if (rc == 0) ThrowLastBaf2SqlError();

            return myArray;
        }

        /// <summary>
        /// Return array 'id', converting to float format.
        /// </summary>        
        public static float[] GetBafFloatArray(UInt64 handle, UInt64 id)
        {
            UInt64 n = 0;
            baf2sql_array_get_num_elements(handle, id, ref n);

            float[] myArray = new float[n];
            int rc = baf2sql_array_read_float(handle, id, myArray);
            if (rc == 0) ThrowLastBaf2SqlError();

            return myArray;
        }

        /// <summary>
        /// Return array 'id', converting to UInt32 format.
        /// </summary>        
        public static UInt32[] GetBafUInt32Array(UInt64 handle, UInt64 id)
        {
            UInt64 n = 0;
            baf2sql_array_get_num_elements(handle, id, ref n);

            UInt32[] myArray = new UInt32[n];
            int rc = baf2sql_array_read_uint32(handle, id, myArray);
            if (rc == 0) ThrowLastBaf2SqlError();

            return myArray;
        }
    }

    [Serializable()]
    public sealed class Baf2SqlException : Exception
    {
        public Baf2SqlException() : base() { }
        public Baf2SqlException(string message) : base(message) { }
        public Baf2SqlException(string message, Exception inner) : base(message, inner) { }
        internal Baf2SqlException(SerializationInfo info, StreamingContext context) { }
    }
}
