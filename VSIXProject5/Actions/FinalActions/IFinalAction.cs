using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using VSIXProject5.Indexers.Models;

namespace VSIXProject5.Actions
{
    //Placeholder for future fun with other commands(rename)
    public interface IFinalAction
    {
        void PrepareAndExecuteGoToQuery(string queryResult, ExpressionResult expressionResult);
        void ExecuteRename();
        void ExecuteNoQueryAtLineFound();
    }
}
