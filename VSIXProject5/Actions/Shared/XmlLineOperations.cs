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

                var elementLocation = DetermineClosestLineWithAttributeByLineNumber(selectedLineNumber, parser.GetStatmentElementsLineNumber());

                string queryAtLine = parser.GetQueryAtLineOrNull(elementLocation);
                if (parser.MapNamespace == null)
                {
                    return queryAtLine;
                }

                return queryAtLine == null ? null: MapNamespaceHelper.CreateFullQueryString(parser.MapNamespace, parser.GetQueryAtLineOrNull(elementLocation));
            }
        }

        private int DetermineClosestLineWithAttributeByLineNumber(int selectionLineNum, List<int> elementsLineNumbers)
        {
            int lineNumber = selectionLineNum + 1;//Missmatch between visual studio lines numeration and text lines numeration
            int? elementLocation = elementsLineNumbers.Cast<int?>().FirstOrDefault(x => x == lineNumber);

            if (elementLocation == null)
            {
                elementsLineNumbers.Add(lineNumber);
                elementsLineNumbers.Sort();
                int indexOfLineNumber = elementsLineNumbers.IndexOf(lineNumber);
                elementLocation = elementsLineNumbers[indexOfLineNumber == 0 ? 0 : indexOfLineNumber - 1];
            }

            return elementLocation.Value;
        }
    }
}
