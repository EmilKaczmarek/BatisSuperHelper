using IBatisSuperHelper.Parsers.Models;
using IBatisSuperHelper.Parsers.XmlConfig.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.Storage.Providers
{
    public class SqlMapConfigProvider : ISqlMapConfigProvider
    {
        private SqlMapConfig _currentSqlMapConfig;
        private GenericStorage<string, SqlMapConfig> _sqlMapConfigs;

        public SqlMapConfigProvider()
        {
            _currentSqlMapConfig = new SqlMapConfig();
            _sqlMapConfigs = new GenericStorage<string, SqlMapConfig>();
        }

        public void SetSingleMapConfig(SqlMapConfig config)
        {
            _sqlMapConfigs.Add(config.Name, config);
            if (_currentSqlMapConfig == null || _currentSqlMapConfig.IsDefault)
            {
                _currentSqlMapConfig = config;
            }
        }

        public void SetMultipleMapConfigs(IEnumerable<SqlMapConfig> configs, SqlMapConfig current)
        {
            _currentSqlMapConfig = current;
            _sqlMapConfigs = new GenericStorage<string, SqlMapConfig>(configs.Select(e=> new KeyValuePair<string, SqlMapConfig>(e.Name, e)));
        }

        public void SetMultipleMapConfigs(IEnumerable<SqlMapConfig> configs, string currentConfigName)
        {
            _sqlMapConfigs = new GenericStorage<string, SqlMapConfig>(configs.Select(e => new KeyValuePair<string, SqlMapConfig>(e.Name, e)));

            if (!_sqlMapConfigs.TryGetValue(currentConfigName, out var sqlMapConfig))
            {
                throw new ArgumentException($"{nameof(currentConfigName)} with value {currentConfigName} was not found.");
            }
            _currentSqlMapConfig = sqlMapConfig;
            
        }

        public Settings GetCurrentSettings()
        {
            return _currentSqlMapConfig.Settings;
        }

        public void SwitchCurrentConfig(SqlMapConfig current)
        {
            if (!_sqlMapConfigs.TryGetValue(current.Name, out var sqlMapConfig))
            {
                throw new ArgumentException($"{nameof(current)} was not found.");
            }
            _currentSqlMapConfig = sqlMapConfig;

        }

        public void SwitchCurrentConfig(string currentConfigName)
        {
            if (_sqlMapConfigs.TryGetValue(currentConfigName, out var sqlMapConfig))
            {
                throw new ArgumentException($"{nameof(currentConfigName)} with value {currentConfigName} was not found.");
            }
            _currentSqlMapConfig = sqlMapConfig;
        }

        public void UpdateOrAddConfig(SqlMapConfig updated)
        {
            if (_sqlMapConfigs.TryGetValue(updated.Name, out var existing))
            {
                _sqlMapConfigs.Update(updated.Name, updated);
            }
            else
            {
                SetSingleMapConfig(updated);
            }

            if (_currentSqlMapConfig.Name == updated.Name)
            {
                SwitchCurrentConfig(updated);
            }
        }
    }
}
