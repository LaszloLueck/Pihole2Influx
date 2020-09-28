using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace dck_pihole2influx.test
{
    public static class TestUtils
    {
        public static string OrderJsonStringFromConvert(string json)
        {
            return JsonConvert.SerializeObject(JArray.Parse(json).OrderBy(token => (string) token["key"]));
        }

        public static Dictionary<string, dynamic> OrderDictionaryFromResult(Dictionary<string, dynamic> inputDictionary)
        {
            return inputDictionary
                .OrderBy(kv => kv.Key)
                .ToDictionary(a => a.Key, a => a.Value);
        }
    }
}