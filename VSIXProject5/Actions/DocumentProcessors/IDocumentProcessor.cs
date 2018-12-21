using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.Actions2.ActionValidators;
using VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolverModels;

namespace VSIXProject5.Actions2.DocumentProcessors
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
