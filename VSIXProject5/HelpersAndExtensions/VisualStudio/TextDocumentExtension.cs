using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.Helpers
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
            var editPoint = document.StartPoint.CreateEditPoint();
            return editPoint.GetText(document.EndPoint);
        }
    }
}
