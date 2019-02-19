using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBatisSuperHelper.Actions.DocumentProcessors;

namespace IBatisSuperHelper.Actions.DocumentProcessors.Factory
{
    public class XmlDocumentProcessorFactory : DocumentProcessorFactory
    {
        public override Task<IDocumentProcessor> GetProcessorAsync(object documentContent, int selectedLineNumber)
        {
            var documentProcessor = new XmlDocumentProcessor(documentContent, selectedLineNumber);
            return documentProcessor.InitializeAsync();
        }
    }
}
