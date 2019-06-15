using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatisSuperHelper.Actions.DocumentProcessors;

namespace BatisSuperHelper.Actions.DocumentProcessors
{
    public abstract class DocumentProcessorFactory
    {
        public abstract Task<IDocumentProcessor> GetProcessorAsync(object documentContent, int selectedLineNumber);
    }
}
