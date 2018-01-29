using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VSIXProject5.Helpers
{
    public class XDocHelper
    {
        public static string GetXDocumentNamespace(IEnumerable<XNode> xDocNodes)
        {
            if(xDocNodes == null || xDocNodes.Count()<1)
                return null;
            
            return ((XElement)xDocNodes.First()).Name.NamespaceName;

        }
        public static string GetXDocumentNamespace(XDocument xDoc)
        {
            if (xDoc == null)
                return null;

            var xDocNodes = xDoc.DescendantNodes();

            return GetXDocumentNamespace(xDocNodes);

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
