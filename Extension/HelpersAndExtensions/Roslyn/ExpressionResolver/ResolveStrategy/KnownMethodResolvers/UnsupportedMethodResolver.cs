using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;

namespace BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolver.ResolveStrategy.KnownMethodResolvers
{
    public class UnsupportedMethodResolver : IKnownMethodResolver
    {
        public ExpressionResult Resolve()
        {
            return new ExpressionResult
            {
                IsSolved = false,
                UnresolvableReason = "Unknow method.",
                TextResult = "",
            };
        }
    }
}
