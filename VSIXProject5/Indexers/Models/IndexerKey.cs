using System.Collections.Generic;

namespace VSIXProject5.Indexers
{
    public class IndexerKey
    {
        public string StatmentName { get; set; }
        public string VsProjectName { get; set; }

        public static IndexerKey ConvertToKey(string statmentName, string vsProjectName)
        {
            return new IndexerKey {
                StatmentName = statmentName,
                VsProjectName= vsProjectName,
            };
        }
        public static bool operator !=(IndexerKey key1, IndexerKey key2)
        {
            return !(key1 == key2);
        }

        public static bool operator ==(IndexerKey key1, IndexerKey key2)
        {
            return EqualityComparer<IndexerKey>.Default.Equals(key1, key2);
        }


        public override bool Equals(object obj)
        {
            var key = obj as IndexerKey;
            return key != null &&
                   StatmentName == key.StatmentName &&
                   VsProjectName == key.VsProjectName;
        }

        public override int GetHashCode()
        {
            var hashCode = 759365612;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(StatmentName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(VsProjectName);
            return hashCode;
        }
    }

}
