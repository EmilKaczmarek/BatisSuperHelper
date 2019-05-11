using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.VSIntegration.DocumentChanges.Actions
{
    public interface IOnFileContentChange
    {
        Task HandleChangeAsync(IWpfTextView textView);
    }
}
