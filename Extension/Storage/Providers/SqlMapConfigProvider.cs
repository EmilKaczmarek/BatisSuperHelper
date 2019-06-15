using BatisSuperHelper.Parsers.Models;
using BatisSuperHelper.Parsers.XmlConfig.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.Storage.Providers
{
    public class SqlMapConfigProvider : ISqlMapConfigProvider
    {
        private GenericStorage<string, SqlMapConfig> _sqlMapConfigs;

        public SqlMapConfigProvider()
        {
            _sqlMapConfigs = new GenericStorage<string, SqlMapConfig>();
        }

        public void AddMultiple(IEnumerable<SqlMapConfig> configs)
        {
            _sqlMapConfigs.AddMultiple(configs.Select(e => new KeyValuePair<string, SqlMapConfig>(e.Name, e)));
        }

        public IEnumerable<SqlMapConfig> GetAll()
        {
            return _sqlMapConfigs.GetAllValues();
        }

        public SqlMapConfig GetSingle(string name)
        {
            return _sqlMapConfigs.GetValue(name);
        }

        public void UpdateOrAddConfig(SqlMapConfig updated)
        {
            if (_sqlMapConfigs.TryGetValue(updated.Name, out var existing))
            {
                _sqlMapConfigs.Update(updated.Name, updated);
            }
            else
            {
                _sqlMapConfigs.Add(updated.Name, updated);
            }
        }

        public SqlMapConfig GetConfigForMapFile(string mapfileName)
        {
            var configs = _sqlMapConfigs.Where(e => e.Value.Maps.Select(x => x.Value).Contains(mapfileName)).Select(e=>e.Value);
            if (configs.Count() == 1)
            {
                return configs.First();
            }

            if (configs.Count() == 0)
            {
                return new SqlMapConfig();
            }

            if (configs.Count() > 1 && configs.All(e => configs.First().Equals(e)))
            {
                return configs.First();
            }

            throw new Exception($"Map {mapfileName} is found in more than 1 non distinct configs.");
        }

    }
}
