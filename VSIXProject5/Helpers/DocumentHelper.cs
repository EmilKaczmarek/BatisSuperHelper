using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.Helpers
{
    public static class DocumentHelper
    {
        public static EnvDTE.Document GetDocumentOrDefault(this ProjectItem projectItem)
        {
            try
            {
                return projectItem.Document;
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                return null;
            }
        }
    }
}
