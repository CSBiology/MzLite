using System.Collections.Generic;
using System.Globalization;
using System.IO;
using MzLite.IO;
using MzLite.Json;
using MzLite.Model;
using MzLite.SQL;
using MzLite.SWATH;
using MzLite.Wiff;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System;
using MzLite.Processing.PeakData;
using MzLite.Processing.Spectrum;
using MzLite.Processing;

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
            {
                var rt = 23.8d;
                var mz1 = 558.3d;
                var mz2 = 776.39d;
                var specIndex = reader.ReadMassSpectra(runID).CreateTargetMzIndex();
                var nearestRt = specIndex.Find(mz1, rt, 0.5, 0.5).ItemAtMin(x => DecimalHelper.AbsDiff(x.Rt, rt));
                var pa = reader.ReadSpectrumPeaks(nearestRt.SourceID);
                var peakIndex = pa.CreateMzIndex(true);
                var maxInt = peakIndex.Find(pa, mz2, 0.05, 0.05).ItemAtMax(x => x.Intensity);
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
