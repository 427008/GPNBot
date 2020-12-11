using GPNBot.API.Models;
using GPNBot.API.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Web;

namespace GPNBot.API.Extensions
{
    public static class StringExtention
    {
        public static bool EqIgnoreCase(this string str, string copmare) => str.Equals(copmare, StringComparison.InvariantCultureIgnoreCase);

        public static Dictionary<string, string> ToDictionary(this string queryString)
        { 
            var nameValueCollection = HttpUtility.ParseQueryString(queryString);
            return nameValueCollection.AllKeys.ToDictionary(key => key, key => nameValueCollection[key]);
        }

        public static List<QueryFilter> QueryFilters (this string queryString)
        { 
            var queryDict = queryString.ToDictionary();
            if(queryDict.TryGetValueIgnoreCase("filter", out var filterStr) && !string.IsNullOrEmpty(filterStr))
            {
                try
                {
                    return JsonSerializer.Deserialize<List<QueryFilter>>(filterStr, GPNJsonSerializer.Option());
                }
                catch
                { 
                }
            }
            return new List<QueryFilter>();
        }

        public static (bool, object) GetValue(this string value, Type type)
        {
            try
            { 
                if(type == typeof(Guid) || type == typeof(Guid?))
                {
                    var guid = Guid.Parse(value);
                    var v = (Guid)Convert.ChangeType(guid, typeof(Guid));
                    return (true, v);
                }

                return (true, Convert.ChangeType(value, type));
            }
            catch
            { 
                return (false, default);
            }
        }

    }
}
