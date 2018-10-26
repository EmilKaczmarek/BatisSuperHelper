using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.VSIntegration.DocumentChanges.Actions
{
    public interface IOnFileContentChange
    {
        void HandleChange(IWpfTextView textView);
    }
}
