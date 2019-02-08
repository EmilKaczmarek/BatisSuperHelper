using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using IBatisSuperHelper.Indexers.Models;

namespace IBatisSuperHelper.Actions
{
    //Placeholder for future fun with other commands(rename)
    public interface IFinalAction
    {
        void PrepareAndExecuteGoToQuery(string queryResult, ExpressionResult expressionResult);
        void ExecuteRename();
        void ExecuteNoQueryAtLineFound();
    }
}
