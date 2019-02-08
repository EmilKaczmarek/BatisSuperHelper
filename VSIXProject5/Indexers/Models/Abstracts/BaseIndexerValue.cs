using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.Indexers.Models
{
    public abstract class BaseIndexerValue
    {
        public string QueryId { get; set; }
        public int QueryLineNumber { get; set; }
        public string QueryFileName { get; set; }
        public string QueryVsProjectName { get; set; }
        public string QueryFilePath { get; set; }

        public override int GetHashCode()
        {
            var hashCode = 814624352;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(QueryId);
            hashCode = hashCode * -1521134295 + QueryLineNumber.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(QueryFileName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(QueryVsProjectName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(QueryFilePath);
            return hashCode;
        }
    }
}
