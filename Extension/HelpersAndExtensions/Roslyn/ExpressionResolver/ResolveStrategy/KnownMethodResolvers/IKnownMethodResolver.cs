using BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolverModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolver.ResolveStrategy.KnownMethodResolvers
{
    public interface IKnownMethodResolver
    {
        ExpressionResult Resolve();
    }
}
