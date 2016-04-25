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
            string wiffPath = @"C:\Work\primaqdev\testdata\C2 Sol IDA.wiff";

            using (var reader = new WiffFileReader(wiffPath))
            {
                WiffNativeID id = new WiffNativeID(0, 0, 0, 10);
                using (var runReader = reader.GetRunReader(""))
                {
                    PeakList pl = runReader.ReadPeakList(id.ToString());
                }
                
            }
        }        

        
    }
}
