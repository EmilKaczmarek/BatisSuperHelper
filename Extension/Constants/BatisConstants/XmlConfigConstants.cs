namespace IBatisSuperHelper.Constants.BatisConstants
{
    public static class XmlConfigConstants
    {
        public const string SettingXPath = "/sqlmapconfig[1]/settings[1]";
        public const string SqlMapXPath = "/sqlmapconfig[1]/sqlmaps[1]";

        public const string UseStatmentNameSpaceAttributeName = "usestatementnamespaces";
        public const string CacheModelsEnabledAttributeName = "cachemodelsenabled";
        public const string ValidateSqlMapAttributeName = "validatesqlmap";
        public const string UseReflectionOptimizerAttributeName = "usereflectionoptimizer";

        public const string ResourceAttributeName = "resource";
        public const string EmbeddedAttributeName = "embedded";
        public const string UriAttributeName = "uri";

        public enum SqlMapResourceType
        {
            UNKNOWN,
            RESOURCE,
            EMBEDDED,
            URI
        }
    }
}
