﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatisSuperHelper.Actions.ActionValidators;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;

namespace BatisSuperHelper.Actions.DocumentProcessors
{
    public interface IDocumentProcessor
    {
        IActionValidator GetValidator();
        ExpressionResult GetQueryValueAtLine(int lineNumber);
        ExpressionResult GetQueryValueAtCurrentSelectedLine();
        IDocumentProcessor Initialize();
        Task<IDocumentProcessor> InitializeAsync();
        bool TryResolveQueryValueAtCurrentSelectedLine(out ExpressionResult expressionResult, out string queryValue);
    }
}
