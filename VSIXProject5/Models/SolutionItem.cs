using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.Models
{
    public class SolutionItem
    {
        public ProjectItem Item { get; set; }
        public string ProjectName { get; set; }
    }
}
