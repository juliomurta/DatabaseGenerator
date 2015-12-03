using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Murta.DatabaseGenerator.Utils
{
    public static class StringUtils
    {
        public static string DeleteLastComma(this string value)
        {
            var indexLastComma = value.LastIndexOf(',');
            return value.Remove(indexLastComma, 1);
        }

        public static StringBuilder DeleteLastComma(this StringBuilder value)
        {
            var newStringBuilder = new StringBuilder();
            newStringBuilder.Append(DeleteLastComma(value.ToString()));
            return newStringBuilder;
        }
    }
}
