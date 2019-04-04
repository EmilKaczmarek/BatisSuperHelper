using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.VSIntegration.ErrorList
{
    public class BatisError
    {
        public string BuildTools => "Batis";
        public string ErrorCategory => "Batis";
        public string ErrorCode { get; set; }
        public string Project { get; set; }
        public __VSERRORCATEGORY ErrorSeverity { get; set; } = __VSERRORCATEGORY.EC_WARNING;
        public VSTASKCATEGORY TaskCategory { get; set; }
        public SnapshotSpan Span { get; set; }
        public string Text { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public TaskPriority Priority => TaskPriority.Normal;
        public TaskCategory Category { get; set; }
        public string Document { get; set; }
    }
}
