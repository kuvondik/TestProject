using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestProject.Models
{
    public class Employee
    {
        protected Dictionary<string, object> properties = new Dictionary<string, object>();

        public object this[string name]
        {
            get
            {
                if (properties.ContainsKey(name))
                    return properties[name];

                return null;
            }

            set
            {
                if (properties[name] != value)
                    properties[name] = value;
            }
        }
    }
}