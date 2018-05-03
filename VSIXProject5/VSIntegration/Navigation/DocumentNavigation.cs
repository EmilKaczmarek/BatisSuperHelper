using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.VSIntegration
{
    public class DocumentNavigation
    {
        private DTE2 _dte;

        public DocumentNavigation(DTE2 dte)
        {
            _dte = dte ?? throw new ArgumentNullException("dte", "Passing null DTE2 is forbidden.");
        }

        public bool OpenDocument(string filePath)
        {
            try
            {
                _dte.ItemOperations.OpenFile(filePath);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void HighlightLineInActiveDocument(int lineNumber)
        {
            TextSelection sel = (TextSelection)_dte.ActiveDocument.Selection;
            TextPoint pnt = (TextPoint)sel.ActivePoint;
            sel.GotoLine(lineNumber, true);
        }
        public void OpenDocumentAndHighlightLine(string filePath, int lineNumber)
        {
            if (OpenDocument(filePath))
            {
                HighlightLineInActiveDocument(lineNumber);
            }
        }
    }
}
