using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Murta.DatabaseGenerator.Console
{
    public class ProviderInfo
    {
        public KeyValuePair<string,string> DatabaseName { get; set; }
        public KeyValuePair<string, string> Username { get; set; }
        public KeyValuePair<string, string> Password { get; set; }
        public KeyValuePair<string, string> ServerInstance { get; set; }
        public KeyValuePair<string, int> Port { get; set; }
        public string ProviderNamespace { get; set; }
    }
}
