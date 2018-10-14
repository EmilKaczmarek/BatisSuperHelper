using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.Windows.ResultWindow.ViewModel
{
    public class ResultWindowViewModel
    {
        public string Query { get; set; }
        public string Namespace { get; set; }
        public int Line { get; set; }
        public string File { get; set; }
        public string FilePath { get; set; }
    }
}
