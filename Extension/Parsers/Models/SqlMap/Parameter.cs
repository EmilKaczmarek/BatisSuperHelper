using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.Parsers.Models.SqlMap
{
    public class Parameter
    {
        public string Name { get; set; }
        public string Column { get; set; }
        public string NullValue { get; set; }
        public string Type { get; set; }
        public string DbType { get; set; }
        public string Size { get; set; }
        public string Scale { get; set; }
        public string Precision { get; set; }
        public string TypeHandler { get; set; }
        public string Direction { get; set; }
    }
}