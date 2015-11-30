using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Murta.DatabaseGenerator.Annotations
{
    public class Column : System.Attribute
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public Column(string name, string type)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(type)) throw new Exception("Invalid arguments.");

            this.Name = name;
            this.Type = type;
        }
    }
}
