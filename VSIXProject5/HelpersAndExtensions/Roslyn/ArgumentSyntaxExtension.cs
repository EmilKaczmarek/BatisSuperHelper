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
        /// <summary>
        ///  
        /// </summary>
        /// <param name="argumentSyntax">Node of ArgumentSyntax type</param>
        /// <returns>Trimed and without special character ToString representation.</returns>
        public static string ToCleanString(this ArgumentSyntax argumentSyntax)
        {
            return argumentSyntax.ToString().Replace("\"", "").Trim();
        } 
    }
}
