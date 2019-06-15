using System.Collections.Generic;
using BatisSuperHelper.Parsers.Models;

namespace BatisSuperHelper.Storage.Providers
{
    public interface ISqlMapConfigProvider
    {
        IEnumerable<SqlMapConfig> GetAll();
        SqlMapConfig GetSingle(string name);
        void AddMultiple(IEnumerable<SqlMapConfig> configs);
        void UpdateOrAddConfig(SqlMapConfig updated);
        SqlMapConfig GetConfigForMapFile(string mapfileName);
    }
}