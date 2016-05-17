using System.Globalization;
using MzLite.Model;
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

        }

        static void Wiff()
        {
            string wiffPath = @"C:\Work\primaqdev\testdata\C2 Sol SWATH4.wiff";
            string runID = "sample=0";

            using (var reader = new WiffFileReader(wiffPath))
            {
                foreach (MassSpectrum ms in reader.ReadMassSpectra(runID))
                {
                    var peaks = reader.ReadSpectrumPeaks(runID, ms.ID);
                    string json = JsonConvert.SerializeObject(ms);
                    MassSpectrum ms2 = JsonConvert.DeserializeObject<MassSpectrum>(json);
                }
            }
        }


    }
}
