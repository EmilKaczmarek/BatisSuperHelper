using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.Helpers
{
    public static class TextDocumentHelper
    {
        /// <summary>
        /// Get full text string from TextDocument
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public static string GetText(this TextDocument document)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            var editPoint = document.StartPoint.CreateEditPoint();
            return editPoint.GetText(document.EndPoint);
        }
    }
}
