using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.Actions.DocumentProcessors;

namespace VSIXProject5.Actions.DocumentProcessors
{
    public abstract class DocumentProcessorFactory
    {
        public abstract Task<IDocumentProcessor> GetProcessorAsync(object documentContent, int selectedLineNumber);
    }
}
