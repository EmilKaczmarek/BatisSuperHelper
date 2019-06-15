using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using BatisSuperHelper.Indexers.Models;

namespace BatisSuperHelper.Actions
{
    //Placeholder for future fun with other commands(rename)
    public interface IFinalAction
    {
        void PrepareAndExecuteGoToQuery(string queryResult, ExpressionResult expressionResult);
        void ExecuteRename();
        void ExecuteNoQueryAtLineFound();
    }
}
