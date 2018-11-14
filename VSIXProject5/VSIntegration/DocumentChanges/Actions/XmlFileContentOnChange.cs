using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using VSIXProject5.Constants;
using VSIXProject5.Indexers;
using VSIXProject5.Parsers;

namespace VSIXProject5.VSIntegration.DocumentChanges.Actions
{
    public class XmlFileContentOnChange : IOnFileContentChange
    {
        public void HandleChange(IWpfTextView textView)
        {
            var snapshot = textView.Caret.Position.BufferPosition.Snapshot;
            ITextDocument textDoc;
            textView.TextBuffer.Properties.TryGetProperty(typeof(ITextDocument), out textDoc);
            using (var stringReader = new StringReader(snapshot.GetText()))
            {
                var project = (Goto.Instance.package as GotoPackage).EnvDTE.Solution.FindProjectItem(textDoc.FilePath).ContainingProject.Name;
                XmlParser parser = XmlParser.WithStringReaderAndFileInfo(stringReader, textDoc.FilePath, project);
                if (parser.XmlNamespace == IBatisConstants.SqlMapNamespace)
                {
                    var newStatments = parser.GetMapFileStatments();
                    Indexer.Instance.UpdateXmlStatmentForFile(newStatments);
                }
            }
        }
    }
}
