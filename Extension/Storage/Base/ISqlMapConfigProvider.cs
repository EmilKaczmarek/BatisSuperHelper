using System.Collections.Generic;
using IBatisSuperHelper.Parsers.Models;
using IBatisSuperHelper.Parsers.XmlConfig.Models;

namespace IBatisSuperHelper.Storage.Providers
{
    public interface ISqlMapConfigProvider
    {
        Settings GetCurrentSettings();
        void SetMultipleMapConfigs(IEnumerable<SqlMapConfig> configs, SqlMapConfig current);
        void SetMultipleMapConfigs(IEnumerable<SqlMapConfig> configs, string currentConfigName);
        void SetSingleMapConfig(SqlMapConfig config);
        void SwitchCurrentConfig(SqlMapConfig current);
        void SwitchCurrentConfig(string currentConfigName);
        void UpdateOrAddConfig(SqlMapConfig updated);
    }
}