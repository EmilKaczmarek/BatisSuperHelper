using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.Actions.DocumentProcessors;

namespace VSIXProject5.Actions.DocumentProcessors.Factory
{
    public class CSharpDocumentProcessorFactory : DocumentProcessorFactory
    {
        public override Task<IDocumentProcessor> GetProcessorAsync(object documentContent, int selectedLineNumber)
        {
            var documentProcessor = new CSharpDocumentProcessor(documentContent, selectedLineNumber);
            return documentProcessor.InitializeAsync();
        }
    }
}
