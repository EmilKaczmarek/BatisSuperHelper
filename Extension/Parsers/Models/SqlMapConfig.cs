using IBatisSuperHelper.Parsers.Models.XmlConfig.SqlMap;
using IBatisSuperHelper.Parsers.XmlConfig.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.Parsers.Models
{
    public class SqlMapConfig
    {
        public bool IsDefault => Settings.Equals(new Settings()) && !Maps.Any();
        public bool ParsedSuccessfully { get; set; }
        public string Name { get; set; }
        public Settings Settings { get; set; } = new Settings();
        public IEnumerable<SqlMap> Maps { get; set; } = Enumerable.Empty<SqlMap>();
        //TODO: Providers, Aliases etc

    }
}
