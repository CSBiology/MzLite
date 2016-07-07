using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace MzLite.Processing
{
    public sealed class CSVRecord
    {

        private readonly IDictionary<string, int> columnIndexer;
        private readonly CultureInfo culture;
        private readonly string[] values;
        public readonly int lineNumber;

        internal CSVRecord(IDictionary<string, int> columnIndexer, int lineNumber, CultureInfo culture, string[] values)
        {
            this.columnIndexer = columnIndexer;
            this.lineNumber = lineNumber;
            this.culture = culture;
            this.values = values;
        }

        public int LineNumber { get { return lineNumber; } }

        public string GetValue(string columnName)
        {
            int idx = -1;

            if (columnIndexer.TryGetValue(columnName, out idx))
                return values[idx];
            else
                throw new KeyNotFoundException(string.Format("Column: '{0}' not found in record at line number: {1}.", columnName, LineNumber));
        }

        public string GetValueNotNullOrEmpty(string columnName)
        {
            string value = GetValue(columnName);
            if (string.IsNullOrWhiteSpace(value))
                throw new FormatException(string.Format("Value in column: '{0}' in record at line number: {1} is empty.", columnName, LineNumber));
            return value;
        }

        /// <summary>
        /// Parse a bool value.
        /// </summary>        
        /// <returns>Returns true if value is 'trueValue', false if value is 'falseValue' or null.</returns>
        public bool? GetBooleanOrNull(string columnName, string trueValue, string falseValue)
        {
            string value = GetValue(columnName);
            if (string.IsNullOrWhiteSpace(value))
                return null;
            if (value.Equals(trueValue, StringComparison.InvariantCultureIgnoreCase))
                return true;
            else if (value.Equals(falseValue, StringComparison.InvariantCultureIgnoreCase))
                return false;
            else
                return null;
        }

        /// <summary>
        /// Parse a bool value.
        /// </summary>        
        /// <returns>Returns true if value is 'trueValue' or false.</returns>
        public bool GetBoolean(string columnName, string trueValue)
        {
            string value = GetValue(columnName);
            return value.Equals(trueValue, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Parse an double value.
        /// </summary>        
        /// <returns>Returns NaN if value is 'nanValue' or null if empty.</returns>
        public double? GetDoubleOrNull(string columnName, string nanValue)
        {
            string value = GetValue(columnName);
            if (string.IsNullOrWhiteSpace(value))
                return null;
            if (value.Equals(nanValue, StringComparison.InvariantCultureIgnoreCase))
                return double.NaN;
            else
                return ParseDouble(columnName, value);

        }

        /// <summary>
        /// Parse an double value.
        /// </summary> 
        public double GetDouble(string columnName)
        {
            string value = GetValueNotNullOrEmpty(columnName);
            return ParseDouble(columnName, value);
        }

        /// <summary>
        /// Parse an double value list.
        /// </summary>        
        /// <returns></returns>
        public double[] GetDoubleArray(string columnName, char sep)
        {
            string value = GetValue(columnName);
            if (string.IsNullOrWhiteSpace(value))
                return new double[0];
            string[] values = value.Split(sep);
            if (values.Length == 0)
                return new double[0];

            double[] array = new double[values.Length];

            for (int i = 0; i < values.Length; i++)
                array[i] = ParseDouble(columnName, values[i]);

            return array;
        }

        /// <summary>
        /// Parse an double value.
        /// </summary>                
        private double ParseDouble(string columnName, string value)
        {
            try
            {
                return double.Parse(value, culture);
            }
            catch (FormatException ex)
            {
                throw new FormatException(string.Format("Number format error in column: '{0}' at line number: {1}.", columnName, LineNumber), ex);
            }
        }

        /// <summary>
        /// Parse an int value.
        /// </summary>        
        /// <returns>Returns null if empty.</returns>
        public int? GetIntOrNull(string columnName)
        {
            string value = GetValue(columnName);
            if (string.IsNullOrWhiteSpace(value))
                return null;
            else
                return ParseInt(columnName, value);
        }

        /// <summary>
        /// Parse an int value.
        /// </summary> 
        public int GetInt(string columnName)
        {
            string value = GetValueNotNullOrEmpty(columnName);
            return ParseInt(columnName, value);
        }

        /// <summary>
        /// Parse an int value.
        /// </summary>                
        private int ParseInt(string columnName, string value)
        {
            try
            {
                return int.Parse(value, culture);
            }
            catch (FormatException ex)
            {
                throw new FormatException(string.Format("Number format error in column: '{0}' at line number: {1}.", columnName, LineNumber), ex);
            }
        }

        public override string ToString()
        {

            string str = string.Empty;

            foreach (var cn in columnIndexer.Keys)
            {
                str += string.Format("{0}='{1}';", cn, GetValue(cn));
            }

            return str;
        }
    }

    public sealed class CSVReader : IDisposable
    {

        private readonly IDictionary<string, int> columnIndex;
        private readonly StreamReader reader;
        private int lineNumber = -1;
        private readonly char separator = '\t';
        private bool isDisposed = false;
        private readonly CultureInfo culture = new CultureInfo("en-US");

        private CSVReader(string filePath, char separator, CultureInfo culture)
        {

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException("filePath");
            if (File.Exists(filePath) == false)
                throw new FileNotFoundException(filePath);

            this.separator = separator;
            this.culture = culture;

            FileInfo fi = new FileInfo(filePath);
            reader = fi.OpenText();

            columnIndex = ReadColumnHeaderIndex(reader, separator);
            lineNumber = 1;
        }

        public static CSVReader GetTabReader(string filePath)
        {
            return new CSVReader(filePath, '\t', new CultureInfo("en-US"));
        }

        /// <summary>
        /// Read all records.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CSVRecord> ReadAll()
        {
            CSVRecord rec;
            while ((rec = ReadNext()) != null)
            {
                yield return rec;
            }
        }

        /// <summary>
        /// Read the next record from stream.
        /// </summary>        
        /// <returns>Next record or null at EOF.</returns>
        public CSVRecord ReadNext()
        {

            if (isDisposed)
                throw new ObjectDisposedException("Can't read record at disposed reader.");

            try
            {
                lineNumber++;

                string line = reader.ReadLine();
                if (line == null)
                    return null; // EOF

                string[] values = line.Split(separator);

                return new CSVRecord(columnIndex, lineNumber, culture, values);
            }
            catch (Exception ex)
            {
                throw new IOException(
                    string.Format(
                    "Parse error at line {0}.",
                    lineNumber),
                    ex);
            }
        }

        /// <summary>
        /// Read the next record from stream.
        /// </summary>        
        /// <returns>Next record or null at EOF.</returns>
        public async Task<CSVRecord> ReadNextAsync()
        {

            if (isDisposed)
                throw new ObjectDisposedException("Can't read record at disposed reader.");

            try
            {
                lineNumber++;

                string line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (line == null)
                    return null; // EOF

                string[] values = line.Split(separator);

                return new CSVRecord(columnIndex, lineNumber, culture, values);
            }
            catch (Exception ex)
            {
                throw new IOException(
                    string.Format(
                    "Parse error at line {0}.",
                    lineNumber),
                    ex);
            }
        }

        /// <summary>
        /// Read the column names from first line.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static IDictionary<string, int> ReadColumnHeaderIndex(StreamReader reader, char separator)
        {
            IDictionary<string, int> columnIndex = new Dictionary<string, int>();

            // read column names
            string line = reader.ReadLine();

            if (line == null)
                throw new IOException("Unexpected end of file.");

            string[] columns = line.Split(separator);

            for (int c = 0; c < columns.Length; c++)
            {
                if (columnIndex.ContainsKey(columns[c]))
                    throw new InvalidOperationException("Double defined column: " + columns[c]);
                columnIndex[columns[c]] = c;
            }

            return columnIndex;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (!isDisposed)
            {
                reader.Close();                
                isDisposed = true;
            }
        }

        #endregion
    }
}
