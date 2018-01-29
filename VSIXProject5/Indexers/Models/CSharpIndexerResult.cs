using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.Indexers.Models
{
    public class CSharpIndexerResult : BaseIndexerValue
    {
        public int HashCode
        {
            get
            {
                return base.GetHashCode();
            }
        }
    }
}
