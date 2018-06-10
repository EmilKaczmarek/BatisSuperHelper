using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.Models;

namespace VSIXProject5.HelpersAndExtensions.Roslyn
{
    public static class RoslynSolutionExtension
    {
        public static List<Document> GetDocument(this Solution solution)
        {
            return solution.Projects.SelectMany(x => x.Documents).ToList();

        }
    }
}
