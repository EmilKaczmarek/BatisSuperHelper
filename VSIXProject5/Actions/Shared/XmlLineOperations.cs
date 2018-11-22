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
        private ITextSnapshot _textSnapshot;
        private int _selectedLineNumber;

        public XmlLineOperations(ITextSnapshot snapshot, int selectedLineNumber)
        {
            _textSnapshot = snapshot;
            _selectedLineNumber = selectedLineNumber;
        }

        public string GetQueryNameAtLine()
        {
            using (var stringReader = new StringReader(_textSnapshot.GetText()))
            {
                XmlParser parser = XmlParser.WithStringReader(stringReader);

                var elementLocation = parser.GetStatmentElementsLineNumber().DetermineClosestInt(_selectedLineNumber + 1);

                return parser.GetQueryAtLineOrNull(elementLocation);
            }
        }

        public bool CanRenameQueryAtLine() => true;
    }
}
