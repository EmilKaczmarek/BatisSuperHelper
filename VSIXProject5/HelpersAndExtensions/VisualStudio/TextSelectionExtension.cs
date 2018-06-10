using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.HelpersAndExtensions.VisualStudio
{
    public static class TextSelectionExtension
    {
        /// <summary>
        /// Places marker at given offsets.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="selection"></param>
        /// <param name="oldBeginOffset"></param>
        /// <param name="oldEndOffset"></param>
        private static void RedoMarker(TextPoint point, TextSelection selection, int oldBeginOffset, int oldEndOffset)
        {
            selection.MoveToLineAndOffset(point.Line, oldBeginOffset);
            selection.MoveToLineAndOffset(point.Line, oldEndOffset, true);
        }
        /// <summary>
        /// Gets full text string from TextSelection.
        /// </summary>
        /// <param name="selection"></param>
        /// <returns></returns>
        public static string GetText(this TextSelection selection)
        {
            TextPoint pnt = (TextPoint)selection.ActivePoint;
            int oldLineCharBeginOffset = pnt.LineCharOffset;
            int oldLineCharEndOffset = selection.AnchorPoint.LineCharOffset;
            selection.GotoLine(pnt.Line, true);
            try
            {  
                return selection.Text;
            }
            finally
            {
                RedoMarker(pnt, selection, oldLineCharBeginOffset, oldLineCharEndOffset);
            }      
        }
    }
}
