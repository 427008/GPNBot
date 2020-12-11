using System.Collections.Generic;


namespace GPNBot.API.Models
{
    public class Condition
    {
        public Condition()
        { 
        }

        public Condition(object _value)
        { 
            Value = _value;
        }

        public string Operator { get; set; } = "=";

        public object Value { get; set; }

        public static Dictionary<string, Condition[]> Create(string key, object value)
        {
            return new Dictionary<string, Condition[]>() 
            {
                { key , new Condition[1] { new Condition(value) } }
            };
        }
    }
}
