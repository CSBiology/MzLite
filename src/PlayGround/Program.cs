using System.Globalization;
using System.IO;
using MzLite.IO;
using MzLite.Model;
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

            //Wiff();
            //SQLite();
            WiffToSQLite();
        }

        static void Wiff()
        {
            string wiffPath = @"C:\Work\primaqdev\testdata\C2 Sol SWATH4.wiff";
            string runID = "sample=0";

            using (var reader = new WiffFileReader(wiffPath))
            {
                foreach (MassSpectrum ms in reader.ReadMassSpectra(runID))
                {
                    var peaks = reader.ReadSpectrumPeaks(ms.ID);
                    string json = JsonConvert.SerializeObject(ms);
                    MassSpectrum ms2 = JsonConvert.DeserializeObject<MassSpectrum>(json);
                }
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
            using (IMzLiteDataWriter writer = new MzLiteSQL(mzLitePath))
            using (ITransactionScope outTxn = writer.BeginTransaction())
            {                
                foreach (var ms in reader.ReadMassSpectra(runID))
                {
                    var peaks = reader.ReadSpectrumPeaks(ms.ID);
                    writer.Insert(runID, ms, peaks);                    
                    break;
                }

                outTxn.Commit();
            }
        }
    }
}
