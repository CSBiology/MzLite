using System.Globalization;
using System.IO;
using MzLite.IO;
using MzLite.Json;
using MzLite.Processing;
using MzLite.SQL;
using MzLite.Wiff;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PlayGround
{
    class Program
    {

        static JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver(),
            Culture = new CultureInfo("en-US")
        };

        static void Main(string[] args)
        {

            Wiff();
            //SQLite();
            //WiffToSQLite();
        }

        static void Wiff()
        {
            string wiffPath = @"C:\Work\primaqdev\testdata\C2 Sol SWATH4.wiff";
            string runID = "sample=0";

            
            using (var reader = new WiffFileReader(wiffPath))
            using (ITransactionScope txn = reader.BeginTransaction())
            using (var swath = SwathReader.Create(reader, runID,true))            
            {

                var ms2Masses = new MzRangeQuery[] { 
                    new MzRangeQuery(776.39d, 0.05, 0.05), 
                    new MzRangeQuery(881.47d, 0.05, 0.05),
                    new MzRangeQuery(689.35d, 0.05, 0.05), 
                    new MzRangeQuery(887.42d, 0.05, 0.05)
                };
                var swathQuery = new SwathQuery(558.3d, 23.8d, 0.1, 0.1, ms2Masses);
                var peaks = swath.GetMS2(swathQuery,false);
                var profile = swath.GetRtProfile(swathQuery, 0, false);
            }
        }

        static void SQLite()
        {
            string mzLitePath = @"C:\Work\primaqdev\testdata\test.mzlite";

            using (var writer = new MzLiteSQL(mzLitePath))
            using(var txn = writer.BeginTransaction())
            {

            }
        }

        static void WiffToSQLite()
        {
            string wiffPath = @"C:\Work\primaqdev\testdata\C2 Sol SWATH4.wiff";
            string mzLitePath = Path.Combine( Path.GetDirectoryName(wiffPath), Path.GetFileNameWithoutExtension(wiffPath) + ".mzlite");
            string runID = "sample=0";

            if (File.Exists(mzLitePath))
                File.Delete(mzLitePath);

            using (IMzLiteDataReader reader = new WiffFileReader(wiffPath))
            using (ITransactionScope inTxn = reader.BeginTransaction())
            using (MzLiteSQL writer = new MzLiteSQL(mzLitePath))
            using (ITransactionScope outTxn = writer.BeginTransaction())
            {                
                foreach (var ms in reader.ReadMassSpectra(runID))
                {
                    var peaks = reader.ReadSpectrumPeaks(ms.ID);
                    var clonedMS = MzLiteJson.JsonCloneModelItem("#1", ms);
                    writer.Insert(runID, clonedMS, peaks);                    
                    break;
                }

                foreach (var ms in writer.ReadMassSpectra(runID))
                {
                    var peaks = writer.ReadSpectrumPeaks(ms.ID);
                    var ms1 = writer.ReadMassSpectrum(ms.ID);
                    break;
                }

                outTxn.Commit();
            }
        }
    }
}
