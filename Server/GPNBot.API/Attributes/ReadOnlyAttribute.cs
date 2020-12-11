using System;


namespace GPNBot.API.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ReadOnlyAttribute : Attribute
    {
    }
}
