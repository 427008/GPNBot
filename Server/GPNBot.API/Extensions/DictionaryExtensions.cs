using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

using GPNBot.API.Models;


namespace GPNBot.API.Extensions
{
    public static class DictionaryExtensions
    {
        public static object ToObject(this Dictionary<string, object> dictionary)
        {
            var eo = new ExpandoObject();
            var eoCollection = (ICollection<KeyValuePair<string, object>>)eo;
            foreach (var kvp in dictionary)
                eoCollection.Add(kvp);

            dynamic eoDynamic = eo;

            return (object)eoDynamic;
        }


        public static bool TryGetValueIgnoreCase(this Dictionary<string, Condition[]> dictionary, string key, out Condition[] value)
        {
            var comparison = StringComparison.CurrentCultureIgnoreCase;
            value = dictionary.Where(x => string.Equals(x.Key, key, comparison))
                        .FirstOrDefault().Value;
            
            return value != default;
        }

        public static void AddValue(this Dictionary<string, Condition[]> dictionary, string key, object value)
        { 
            if(dictionary.ContainsKey(key))
                dictionary[key] = dictionary[key].Append(new Condition(value)).ToArray();
            else
                dictionary[key] = new Condition[1] { new Condition(value) };
        }
        public static void AddCondition(this Dictionary<string, Condition[]> dictionary, string key, Condition condition)
        { 
            if(dictionary.ContainsKey(key))
                dictionary[key] = dictionary[key].Append(condition).ToArray();
            else
                dictionary[key] = new Condition[1] { condition };
        }


        public static bool TryGetValueIgnoreCase(this Dictionary<string, object> dictionary, string key, out object value)
        {
            var comparison = StringComparison.CurrentCultureIgnoreCase;
            var tmp = dictionary.Where(x => string.Equals(x.Key, key, comparison))
                        .Select(x => new { x.Value })
                        .FirstOrDefault();
            value = tmp?.Value;

            return tmp != null;
        }
        public static bool TryGetValueIgnoreCase(this Dictionary<string, string> dictionary, string key, out string value)
        {
            var comparison = StringComparison.CurrentCultureIgnoreCase;
            var tmp = dictionary.Where(x => string.Equals(x.Key, key, comparison))
                        .Select(x => new { x.Value })
                        .FirstOrDefault();
            value = tmp?.Value;

            return tmp != null;
        }
    }
}
