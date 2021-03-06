﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatisSuperHelper.Actions.DocumentProcessors;

namespace BatisSuperHelper.Actions.DocumentProcessors.Factory
{
    public class XmlDocumentProcessorFactory : DocumentProcessorFactory
    {
        public async override Task<IDocumentProcessor> GetProcessorAsync(object documentContent, int selectedLineNumber)
        {
            var documentProcessor = new XmlDocumentProcessor(documentContent, selectedLineNumber);
            return await documentProcessor.InitializeAsync();
        }
    }
}
