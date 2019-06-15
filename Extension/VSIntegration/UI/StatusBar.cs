using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.VSIntegration
{
    public class StatusBarIntegration
    {
        private readonly IVsStatusbar _statusBar;
        public StatusBarIntegration(IVsStatusbar statusBar)
        {
            _statusBar = statusBar;
        }
        public void ShowText(string text)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            if (string.IsNullOrEmpty(text))
                return;

            int frozen;
            _statusBar.IsFrozen(out frozen);
            if (frozen == 0)
            {
                _statusBar.SetText(text);
            }
        }
    }
}
