using System.Globalization;
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
            

            MzLiteProject project = new MzLiteProject("test project");
            project.Samples.Add(new Sample("test1"));
            Sample s = project.Samples["test1"];
            project.Samples.Rename(s, "test2");
            s = project.Samples["test2"];
            Run run = new Run("test run");
            project.Runs.Add(run);
            run.Sample = s;

            var json = JsonConvert.SerializeObject(project, jsonSettings);
            project = JsonConvert.DeserializeObject<MzLiteProject>(json, jsonSettings);
        }
    }
}
