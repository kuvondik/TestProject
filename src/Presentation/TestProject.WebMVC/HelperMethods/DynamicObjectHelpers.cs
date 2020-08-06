using System.Collections.Generic;
using System.Dynamic;

namespace TestProject.WebMVC.HelperMethods
{
    public static class DynamicObjectHelpers
    {
        public static List<string> GetPropertyNames(ExpandoObject obj)
        {
            var propNames = new List<string>();
            foreach (string o in ((IDictionary<string, object>)obj)?.Keys)
            {
                // If property name has dots, get the part of string after the last dot.
                var propNameSplits = o.ToString().Split('.');
                var propName = propNameSplits[^1];

                propNames.Add(propName);
            }

            return propNames;
        }

        public static List<string> GetPropertyValues(ExpandoObject obj)
        {
            var propValues = new List<string>();
            foreach (var o in (obj as IDictionary<string, object>)?.Values)
                propValues.Add(o?.ToString());

            return propValues;
        }
    }
}