using System.Globalization;
using System.IO;
using MzLite.IO;
using MzLite.Json;
using MzLite.Processing;
using MzLite.SQL;
using MzLite.Thermo;
using MzLite.Wiff;
using MzLite.MetaData;
using MzLite.MetaData.PSIMS;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Diagnostics;
using System;

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
            //Thermo();
            TestSwath();
            //TestRt();
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
                var swath = SwathIndexer.Create(reader, runID);
                var ms2Masses = new RangeQuery[] { 
                    new RangeQuery(776.39d, 0.05, 0.05), 
                    new RangeQuery(881.47d, 0.05, 0.05),
                    new RangeQuery(689.35d, 0.05, 0.05), 
                    new RangeQuery(887.42d, 0.05, 0.05)
                };
                var swathQuery = new SwathQuery(558.3d, new RangeQuery(23.8d, 0.1), ms2Masses);
                var peaks = swath.GetMS2(reader, swathQuery, false);
                var profile = swath.GetRtProfile(reader, swathQuery, 0, false);
            }
        }

        static void Thermo()
        {

            string rawPath = @"C:\Work\primaqdev\testdata\06042010HSRE3mem117.RAW";
            string runID = "run_1";
            int msLevel;

            using (var reader = new ThermoRawFileReader(rawPath))
            using (ITransactionScope txn = reader.BeginTransaction())
            {

                

                foreach (var ms in reader.ReadMassSpectra(runID))
                {
                    if (ms.TryGetMsLevel(out msLevel) && msLevel != 2)
                        continue;

                    var json = MzLiteJson.ToJson(ms);
                    var peaks = reader.ReadSpectrumPeaks(ms.ID);                    
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

        static void TestSwath()
        {

            string wiffPath = @"C:\Work\primaqdev\testdata\swathtest\20141201_BASF5_1SW01.wiff";
            string searchListPath = @"C:\Work\primaqdev\testdata\swathtest\result_N14 - Ions.txt";
            string runID = "sample=0";

            using (var reader = new WiffFileReader(wiffPath))
            using (ITransactionScope txn = reader.BeginTransaction())
            using (var csv = CSVReader.GetTabReader(searchListPath))
            {
                
                var swath = SwathIndexer.Create(reader, runID);

                var queuries = csv.ReadAll()
                    .GroupBy(x => x.GetDouble("Precursor MZ"))
                    .Select(x => CreateSwathQuery(x, 1.0, 0.05))
                    .ToArray();

                SwathQuerySorting.Sort(queuries);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                for (int i = 0; i < queuries.Length; i++ )
                {
                    if(i % 10 == 0)
                        Console.WriteLine(i);

                    var profile = swath.GetRtProfiles(reader, queuries[i], true);

                    if (profile.Length > 0)
                    {
                        //var sum = profile.Sum(x => x.Intensity);
                    }

                }

                stopwatch.Stop();
                Console.WriteLine("Elapsed time: {0}", stopwatch.Elapsed.ToString());

                Console.WriteLine("Press any key to exit.");
                System.Console.ReadKey();

            }

        }

        static void TestRt()
        {
            string wiffPath = @"C:\Work\primaqdev\testdata\swathtest\20141201_BASF5_1SW01.wiff";
            string searchListPath = @"C:\Work\primaqdev\testdata\swathtest\result_N14 - Ions.txt";
            string runID = "sample=0";

            using (var reader = new WiffFileReader(wiffPath))
            using (ITransactionScope txn = reader.BeginTransaction())
            using (var csv = CSVReader.GetTabReader(searchListPath))
            {
                var rti = RtIndexer.Create(reader, runID, 1);

                var queuries = csv.ReadAll()
                    .GroupBy(x => x.GetDouble("Precursor MZ"))
                    .Select(x => CreateRtQuery(x, 1.0, 0.01))
                    .ToArray();

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                for (int i = 0; i < queuries.Length; i++)
                {
                    if (i % 10 == 0)
                        Console.WriteLine(i);

                    var query = queuries[i];

                    int idx = rti.GetMaxIntensityIndex(reader, query);
                    if (idx >= 0)
                    {
                        var p = rti.GetClosestMz(reader, idx, query.MzRange, true);
                    }

                    var mt = rti.GetMassTrace(reader, query, false);

                    if (mt.Length > 0)
                    {
                        //var sum = profile.Sum(x => x.Intensity);
                    }

                }

                stopwatch.Stop();
                Console.WriteLine("Elapsed time: {0}", stopwatch.Elapsed.ToString());

                Console.WriteLine("Press any key to exit.");
                System.Console.ReadKey();
            }
        }
        
        static SwathQuery CreateSwathQuery(IGrouping<double, CSVRecord> targetMzGroup, double rtOffset, double ms2mzOffset)
        {
            double targetMz = targetMzGroup.Key;
            double rt = targetMzGroup.First().GetDouble("RT");
            RangeQuery[] ms2Masses = targetMzGroup
                .Select(x => new RangeQuery(x.GetDouble("Fragment MZ"), ms2mzOffset))
                .ToArray();
            return new SwathQuery(targetMz, new RangeQuery(rt, rtOffset), ms2Masses);
        }

        static RtIndexerQuery CreateRtQuery(IGrouping<double, CSVRecord> targetMzGroup, double rtOffset, double mzOffset)
        {
            double rt = targetMzGroup.First().GetDouble("RT");
            double mz = targetMzGroup.Key;
            return new RtIndexerQuery(new RangeQuery(rt, rtOffset), new RangeQuery(mz, mzOffset));
        }
    }
}
