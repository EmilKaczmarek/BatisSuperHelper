using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.VSIntegration
{
    public class StatusBarIntegration
    {
        private IVsStatusbar _statusBar;
        public StatusBarIntegration(IVsStatusbar statusBar)
        {
            _statusBar = statusBar;
        }
        public void ShowText(string text)
        {
            int frozen;
            _statusBar.IsFrozen(out frozen);
            if (frozen == 0)
            {
                _statusBar.SetText(text);
            }
        }
    }
}
