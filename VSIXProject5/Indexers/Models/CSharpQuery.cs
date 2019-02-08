using Microsoft.CodeAnalysis;

namespace IBatisSuperHelper.Indexers.Models
{
    public class CSharpQuery : BaseIndexerValue
    {
        public DocumentId DocumentId { get; set; }
        public bool IsGeneric { get; set; }
        public int HashCode
        {
            get
            {
                return base.GetHashCode();
            }
        }
    }
}
