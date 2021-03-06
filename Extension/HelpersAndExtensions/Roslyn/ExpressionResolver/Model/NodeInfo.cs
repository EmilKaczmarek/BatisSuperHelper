﻿using Microsoft.CodeAnalysis;

namespace BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolver.Model
{
    public class NodeInfo
    {
        public MethodInfo MethodInfo { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public string ProjectName { get; set; }
        public DocumentId DocumentId { get; set; }
    }
}
