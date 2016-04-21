using System.Globalization;
using MzLite.Binary;
using MzLite.Model;
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

            MassSpectrum ms = new MassSpectrum("test");

            double[] values = { 1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7, 8.8, 9.9, 10.01 };

            using (var encoder = new BinaryDataEncoder())
            {
                ms.PeakArray.CompressionType = BinaryDataCompressionType.ZLib;
                IPeakEnumerable peaks = new DataArrayEnumerable(values, values);                
                byte[] bytes = encoder.Encode(ms.PeakArray, peaks);
                bytes = encoder.Encode(ms.PeakArray, peaks);
                BinaryDataDecoder decoder = new BinaryDataDecoder();
                peaks = decoder.Decode(ms.PeakArray, bytes);
            }

            var json = JsonConvert.SerializeObject(ms, jsonSettings);
            ms = JsonConvert.DeserializeObject<MassSpectrum>(json, jsonSettings);

            //MzLiteProject project = new MzLiteProject("test project");
            //project.Samples.Add(new Sample("test1"));
            //Sample s = project.Samples["test1"];
            //project.Samples.Rename(s, "test2");
            //s = project.Samples["test2"];
            //Run run = new Run("test run");
            //project.Runs.Add(run);
            //run.Sample = s;

            //var json = JsonConvert.SerializeObject(project, jsonSettings);
            //project = JsonConvert.DeserializeObject<MzLiteProject>(json, jsonSettings);
        }
    }
}
