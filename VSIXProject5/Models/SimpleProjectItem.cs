using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.Models
{
    public class SimpleProjectItem
    {
        public string FilePath { get; set; }
        public string ProjectName { get; set; }
        public bool IsCSharpFile { get; set;}
        public Document RoslynDocument { get; set; }
    }
}
