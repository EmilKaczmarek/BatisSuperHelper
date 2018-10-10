using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.HelpersAndExtensions;
using VSIXProject5.Parsers;

namespace VSIXProject5.Actions.Shared
{
    public class XmlLineOperations : ILineOperation
    {
        public string GetQueryNameAtLine(ITextSnapshot snapshot, int selectedLineNumber)
        {
            using (var stringReader = new StringReader(snapshot.GetText()))
            {
                XmlParser parser = XmlParser.WithStringReader(stringReader);

                var elementLocation = parser.GetStatmentElementsLineNumber().DetermineClosestInt(selectedLineNumber+1);

                return parser.GetQueryAtLineOrNull(elementLocation);
            }
        }
    }
}
