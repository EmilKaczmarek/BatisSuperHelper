using IBatisSuperHelper.Constants.BatisConstants;

namespace IBatisSuperHelper.Parsers.XmlConfig.Models
{
    public class Settings
    {
        public bool UseStatementNamespaces { get; set; } = false;
        public bool CacheModelsEnabled { get; set; } = true;
        public bool ValidateSqlMap { get; set; } = false;
        public bool UseReflectionOptimizer { get; set; } = true;

        public void ApplyFromAttribute(string attributeName, string attributeValue)
        {
            switch (attributeName) {
                case XmlConfigConstants.UseStatmentNameSpaceAttributeName:
                    UseStatementNamespaces = bool.TryParse(attributeValue, out bool useStatementNamespacesValue) ? useStatementNamespacesValue : UseStatementNamespaces;
                    break;
                case XmlConfigConstants.CacheModelsEnabledAttributeName:
                    CacheModelsEnabled = bool.TryParse(attributeValue, out bool cacheModelsEnabledValue) ? cacheModelsEnabledValue : CacheModelsEnabled;
                    break;
                case XmlConfigConstants.ValidateSqlMapAttributeName:
                    ValidateSqlMap = bool.TryParse(attributeValue, out bool validateSqlMapValue) ? validateSqlMapValue : ValidateSqlMap;
                    break;
                case XmlConfigConstants.UseReflectionOptimizerAttributeName:
                    UseReflectionOptimizer = bool.TryParse(attributeValue, out bool useReflectionOptimizerValue) ? useReflectionOptimizerValue : UseReflectionOptimizer;
                    break;
            }
        }

        public override bool Equals(object obj)
        {
            var setting = obj as Settings;
            return setting != null &&
                   UseStatementNamespaces == setting.UseStatementNamespaces &&
                   CacheModelsEnabled == setting.CacheModelsEnabled &&
                   ValidateSqlMap == setting.ValidateSqlMap &&
                   UseReflectionOptimizer == setting.UseReflectionOptimizer;
        }
    }
}