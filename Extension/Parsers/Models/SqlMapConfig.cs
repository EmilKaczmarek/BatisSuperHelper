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

        public override bool Equals(object obj)
        {
            var config = obj as SqlMapConfig;
            return config != null &&
                   IsDefault == config.IsDefault &&
                   ParsedSuccessfully == config.ParsedSuccessfully &&
                   Name == config.Name &&
                   EqualityComparer<Settings>.Default.Equals(Settings, config.Settings) &&
                   EqualityComparer<IEnumerable<SqlMap>>.Default.Equals(Maps, config.Maps);
        }

        public override int GetHashCode()
        {
            var hashCode = 1927817751;
            hashCode = hashCode * -1521134295 + IsDefault.GetHashCode();
            hashCode = hashCode * -1521134295 + ParsedSuccessfully.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<Settings>.Default.GetHashCode(Settings);
            hashCode = hashCode * -1521134295 + EqualityComparer<IEnumerable<SqlMap>>.Default.GetHashCode(Maps);
            return hashCode;
        }
    }
}
