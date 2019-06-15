using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.VSIntegration.ErrorList
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

        public string TaggerErrorType => GetTaggerError();

        private string GetTaggerError()
        {
            switch (ErrorSeverity)
            {
                case __VSERRORCATEGORY.EC_ERROR:
                    return PredefinedErrorTypeNames.CompilerError;
                case __VSERRORCATEGORY.EC_WARNING:
                    return PredefinedErrorTypeNames.Warning;
                case __VSERRORCATEGORY.EC_MESSAGE:
                    return PredefinedErrorTypeNames.Suggestion;
                default:
                    return PredefinedErrorTypeNames.OtherError;
            }
        }
    }
}
