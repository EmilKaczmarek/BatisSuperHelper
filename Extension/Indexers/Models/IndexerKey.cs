using System.Collections.Generic;

namespace BatisSuperHelper.Indexers
{
    public class IndexerKey
    {
        public string StatmentName { get; set; }
        public string VsProjectName { get; set; }
        public string StatmentFullyQualifiedName { get; set; }

        public static IndexerKey ConvertToKey(string statmentName, string vsProjectName, string mapNamespace)
        {
            return new IndexerKey {
                StatmentName = statmentName,
                VsProjectName = vsProjectName,
                StatmentFullyQualifiedName = mapNamespace,
            };
        }

        public override bool Equals(object obj)
        {
            var key = obj as IndexerKey;
            return key != null &&
                   StatmentName == key.StatmentName &&
                   VsProjectName == key.VsProjectName &&
                   StatmentFullyQualifiedName == key.StatmentFullyQualifiedName;
        }

        public override int GetHashCode()
        {
            var hashCode = 1782398838;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(StatmentName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(VsProjectName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(StatmentFullyQualifiedName);
            return hashCode;
        }

        public static bool operator ==(IndexerKey key1, IndexerKey key2)
        {
            return EqualityComparer<IndexerKey>.Default.Equals(key1, key2);
        }

        public static bool operator !=(IndexerKey key1, IndexerKey key2)
        {
            return !(key1 == key2);
        }
    }

}
