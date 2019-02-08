using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.VSIntegration
{
    public class DocumentNavigation
    {
        private DTE2 _dte;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="dte">Not null dte.</param>
        public DocumentNavigation(DTE2 dte)
        {
            _dte = dte ?? throw new ArgumentNullException(nameof(DTE2), "Passing null DTE2 is forbidden.");
        }
        /// <summary>
        /// Opens tab with file content.
        /// </summary>
        /// <param name="filePath">Path to file.</param>
        /// <returns></returns>
        public bool OpenDocument(string filePath)
        {
            try
            {
                _dte.ItemOperations.OpenFile(filePath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// Highlights all characters in line.
        /// </summary>
        /// <param name="lineNumber">Line number</param>
        public void HighlightLineInActiveDocument(int lineNumber)
        {
            TextSelection sel = (TextSelection)_dte.ActiveDocument.Selection;
            TextPoint pnt = sel.ActivePoint;
            sel.GotoLine(lineNumber, true);
        }
        /// <summary>
        /// Opens file and highlightes all characters in given line.
        /// </summary>
        /// <param name="filePath">File Path</param>
        /// <param name="lineNumber">Line number</param>
        public void OpenDocumentAndHighlightLine(string filePath, int lineNumber)
        {
            if (OpenDocument(filePath))
            {
                HighlightLineInActiveDocument(lineNumber);
            }
        }
    }
}
