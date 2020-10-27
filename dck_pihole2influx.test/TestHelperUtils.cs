using System;
using System.Linq;
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
        
        public static Option<string> OrderJsonObjectString(string json)
        {
            try
            {
                var result = new JObject(JObject.Parse(json).Properties().OrderBy(p => p.Name)).ToString();
                return Option.Some(result);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "An error occured!");
                return Option.None<string>();
            }
        }

        public static Option<string> OrderJsonArrayString(string json, string orderBy)
        {
            try
            {
                var result = new JArray(JArray.Parse(json).OrderBy(obj => ((JObject)obj)[orderBy])).ToString();
                return Option.Some(result);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "An error occured!");
                return Option.None<string>();
            }            
        }
        
    }
}