using System.Collections.Generic;


namespace GPNBot.API.Models
{
    public class CSSet
    {
        private Dictionary<string, object> _csSet;

        public void Add(string key)
        { 
            if(_csSet is null)
                _csSet = new Dictionary<string, object> { { key, null } };
            else
            { 
                if(!_csSet.ContainsKey(key))
                    _csSet[key] = null;
            }
        }

        public bool HasKey(string key)
        { 
            if(_csSet is null) 
                return false;

            return _csSet.ContainsKey(key);
        }
    }
}
