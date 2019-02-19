using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBatisSuperHelper.Actions.DocumentProcessors;

namespace IBatisSuperHelper.Actions.DocumentProcessors
{
    public abstract class DocumentProcessorFactory
    {
        public abstract Task<IDocumentProcessor> GetProcessorAsync(object documentContent, int selectedLineNumber);
    }
}
