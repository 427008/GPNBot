using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GPNBot.API.Attributes;


namespace GPNBot.API.Extensions
{
    public static class TypeExtension
    {
        public static string GetSelectableFieldListStr(this Type model)
        {
            return string.Join(",", model
                .GetProperties()
                .Where(p => Attribute.GetCustomAttribute(p, typeof(PersistAttribute)) != null &&
                            p.GetAccessors().FirstOrDefault(m =>m.IsStatic) is null)
                .Select(p => p.Name)
                .ToList());
        }
        public static string GetReadOnlyFieldListStr(this Type model)
        {
            return string.Join(",", model
                .GetProperties()
                .Where(p => Attribute.GetCustomAttribute(p, typeof(ReadOnlyAttribute)) != null &&
                            p.GetAccessors().FirstOrDefault(m =>m.IsStatic) is null)
                .Select(p => p.Name)
                .ToList());
        }

        public static List<PropertyInfo> GetKeyList(this Type model)
        { 
            return model
                .GetProperties()
                .Where(p => Attribute.GetCustomAttribute(p, typeof(KeyAttribute)) != null)
                .ToList();
        }
    }
}