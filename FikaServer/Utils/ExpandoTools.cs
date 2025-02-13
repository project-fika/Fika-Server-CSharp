using System.Dynamic;

namespace FikaServer.Utils
{
    public static class ExpandoTools
    {
        public static ExpandoObject ToDynamicObject(this object obj)
        {
            ExpandoObject expandoObject = new ExpandoObject();

            if (obj == null)
            {
                return expandoObject;
            }

            RouteValueDictionary dictionaryValues = new RouteValueDictionary(obj);
            if (dictionaryValues.Count == 0)
            {
                return expandoObject;
            }

            IDictionary<string, object> expandoDictionary = expandoObject as IDictionary<string, object>;
            foreach (var value in dictionaryValues)
            {
                expandoDictionary.Add(value.Key, value.Value);
            }

            return expandoObject;
        }

        public static void AddOrReplaceProperty(this ExpandoObject obj, string key, object value)
        {
            if (obj == null)
            {
                return;
            }

            IDictionary<string, object> expandoDictionary = obj as IDictionary<string, object>;

            if (expandoDictionary.ContainsKey(key)) 
            {
                expandoDictionary[key] = value;
            }
            else
            {
                expandoDictionary.Add(key, value);
            }
        }
    }
}
