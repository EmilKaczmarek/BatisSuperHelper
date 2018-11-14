using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace VSIXProject5.Loggers
{
    public static class OutputWindowLogger
    {
        private static IVsOutputWindowPane _outputWindowPane;
        private static SVsOutputWindow _SVsOutputWindow;
        private static readonly string _tabName = "iBatis Super Helper";

        public static void Init(SVsOutputWindow outputWindow)
        {
            _SVsOutputWindow = outputWindow;
        }

        public static void WriteLn(string logMessage)
        {
            if (EnsurePane())
            {
                _outputWindowPane.OutputString($"{logMessage}{Environment.NewLine}");
            }
        }

        private static bool EnsurePane()
        {
            if (_outputWindowPane == null)
            {
                Guid guid = Guid.NewGuid();
                var outputWindow = (IVsOutputWindow)_SVsOutputWindow;
                outputWindow.CreatePane(ref guid, _tabName, 1, 1);
                outputWindow.GetPane(ref guid, out _outputWindowPane);
            }
            return _outputWindowPane != null;
        }
    }
}
