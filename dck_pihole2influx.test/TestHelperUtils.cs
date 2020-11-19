using System;
using System.Linq;
using System.Threading.Tasks;
using dck_pihole2influx.Logging;
using Newtonsoft.Json.Linq;
using Optional;

namespace dck_pihole2influx.test
{
    public class TestHelperUtils
    {

        protected TestHelperUtils()
        {
            
        }
        
        private static readonly IMySimpleLogger Logger = MySimpleLoggerImpl<TestHelperUtils>.GetLogger();

        protected static Option<string> OrderJsonObjectString(string json)
        {
            try
            {
                var result = new JObject(JObject.Parse(json).Properties().OrderBy(p => p.Name)).ToString();
                return Option.Some(result);
            }
            catch (Exception exception)
            {
                Task.Run(async () =>
                {
                    await Logger.ErrorAsync(exception, "An error occured!");
                });
                return Option.None<string>();
            }
        }

        protected static Option<string> OrderJsonArrayString(string json, string orderBy)
        {
            try
            {
                var result = new JArray(JArray.Parse(json).OrderBy(obj => ((JObject)obj)[orderBy])).ToString();
                return Option.Some(result);
            }
            catch (Exception exception)
            {
                Task.Run(async () =>
                {
                    await Logger.ErrorAsync(exception, "An error occured!");
                });
                return Option.None<string>();
            }            
        }
        
    }
}