using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.HelpersAndExtensions.VisualStudio
{
    public static class SnapshotExtension
    {
        public static string GetContentTypeName(this ITextSnapshot snapshot)
        {
            return snapshot.ContentType.TypeName;
        }
        public static bool IsCSharpType(this ITextSnapshot snapshot)
        {
            return snapshot.GetContentTypeName() == "CSharp";
        }
    }
}
