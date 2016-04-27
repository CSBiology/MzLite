using System.Globalization;
using MzLite.Json;
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

            using (var reader = new WiffFileReader(wiffPath))
            {                
                using (var runReader = reader.GetRunReader("sample=0"))
                {
                    foreach (MassSpectrum ms in runReader.ReadMassSpectra())
                    {
                        var peaks = runReader.ReadSpectrumPeaks(ms.ID);                        
                        string json = JsonConvert.SerializeObject(ms);
                        MassSpectrum ms2 = JsonConvert.DeserializeObject<MassSpectrum>(json);
                    }
                }
                
            }
        }        

        
    }
}
