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
        private static void RedoMarker(TextPoint pnt, TextSelection selection, int oldBeginOffset, int oldEndOffset)
        {
            selection.MoveToLineAndOffset(pnt.Line, oldBeginOffset);
            selection.MoveToLineAndOffset(pnt.Line, oldEndOffset, true);
        }

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
