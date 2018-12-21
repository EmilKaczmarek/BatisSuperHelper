using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolverModels;

namespace VSIXProject5.Indexers.Models
{
    public class CSharpIndexerResult
    {
        public List<CSharpQuery> Queries { get; set; }
        public List<ExpressionResult> Generics { get; set; }
    }
}
