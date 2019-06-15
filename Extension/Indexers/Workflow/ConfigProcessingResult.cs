using BatisSuperHelper.Parsers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.Indexers.Workflow
{
    public class ConfigProcessingResult
    {
        public bool HasAtLeastOneProperConfig { get; set; }
        public bool HasMultipleProperConfigs { get; set; }
        public bool FallbackDefaultConfigUsed { get; set; }
        public bool FallbackFirstConfigUsed { get; set; }

        public static ConfigProcessingResult FallbackDefault => new ConfigProcessingResult
        {
            FallbackDefaultConfigUsed = true,
        };

        public static ConfigProcessingResult FallbackFirst => new ConfigProcessingResult
        {
            FallbackDefaultConfigUsed = true,
        };

        public static ConfigProcessingResult FromProcessingResults(IEnumerable<SqlMapConfig> configs)
        {
            var result = new ConfigProcessingResult();
            
            if (configs.Count() == 1)
            {
                result.HasAtLeastOneProperConfig = true;
            }

            if (configs.Count() > 1)
            {
                result.HasMultipleProperConfigs = true;
            }

            return result;
        }
    }
}
