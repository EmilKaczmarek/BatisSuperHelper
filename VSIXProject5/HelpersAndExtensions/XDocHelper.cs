using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VSIXProject5.Helpers
{
    public static class XDocHelper
    {
        public static string GetXDocumentNamespace(XDocument xDoc)
        {
            if (xDoc == null)
                return null;

            if(xDoc.Root == null)
                return null;

            string rootNamespace = xDoc.Root.GetDefaultNamespace().ToString();

            return rootNamespace;
        }
        public static string GetXDocumentNamespace(string filePath)
        {
            XDocument xdoc = null;
            try
            {
                 xdoc = XDocument.Load(filePath);
            }
            catch(Exception)//Change it to more describing exception type.
            {
                return null;
            }
            return GetXDocumentNamespace(xdoc);
        }
    }
}
