using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.HelpersAndExtensions.Roslyn
{
    public static class ArgumentSyntaxExtension
    {
        public static string ToCleanString(this ArgumentSyntax argumentSyntax)
        {
            return argumentSyntax.ToString().Replace("\"", "").Trim();
        } 
    }
}
