using IBatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using IBatisSuperHelper.Indexers.Models;
using IBatisSuperHelper.Storage.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.Actions
{
    public abstract class BaseFinalEventActionsExecutor<T> where T: BaseFinalEventActionsExecutor<T>
    {
        internal Dictionary<Type, dynamic> QueryDataServices = new Dictionary<Type, dynamic>();
        internal Dictionary<Type, dynamic> LogicHandlers = new Dictionary<Type, dynamic>();
        internal bool UseNamespace;

        protected BaseFinalEventActionsExecutor() { }

        public virtual T WithQueryDataService(Type type, dynamic queryDataService)
        {
            QueryDataServices.Add(type, queryDataService);
            return (T)this;
        }

        public virtual T WithLogicHandler(Type type, dynamic logicHandler)
        {
            LogicHandlers.Add(type, logicHandler);
            return (T)this;
        }


        public virtual T WithUseNamespace(bool useNamespace)
        {
            UseNamespace = useNamespace;
            return (T)this;
        }

        public abstract void Execute(string queryResult, ExpressionResult expressionResult);
    }
}
