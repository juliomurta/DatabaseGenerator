using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Murta.DatabaseGenerator.Annotations
{
    public class Table : System.Attribute
    {
        public string Name { get; set; }

        public Table(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new Exception("Invalid argument");

            this.Name = name;
        }
    }
}
