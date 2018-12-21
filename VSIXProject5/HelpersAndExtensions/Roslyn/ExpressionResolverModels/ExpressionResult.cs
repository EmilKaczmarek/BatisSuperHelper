﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.HelpersAndExtensions.Roslyn.ExpressionResolverModels
{
    public enum UnresolvedPartType
    {
        None,
        ClassName,
    }

    public class ExpressionResult
    {
        public bool IsSolved { get; set; }
        public string UnresolvableReason { get; set; }
        public string TextResult { get; set; }
        public int CallsNeeded { get; set; }
        public string ExpressionText { get; set; }
        public bool CanBeUsedAsQuery { get; set; }
        public string UnresolvedValue { get; set; }
        public UnresolvedPartType UnresolvedPart { get; set; }
        public string UnresolvedFormat { get; set; }
        public string MethodName { get; set; }

        public string Resolve(string arg0)
        {
            if (this.IsSolved)
            {
                return TextResult;
            }

            return string.Format(this.UnresolvedFormat, arg0);
        }
    }
}
