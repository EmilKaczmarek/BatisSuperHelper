using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.HelpersAndExtensions.Roslyn.ExpressionResolver.Model
{
    public class MethodInfo
    {
        public string MethodName { get; set; }
        public string MethodClass { get; set; }

        public override bool Equals(object obj)
        {
            var info = obj as MethodInfo;
            return info != null &&
                   MethodName == info.MethodName &&
                   MethodClass == info.MethodClass;
        }

        public override int GetHashCode()
        {
            var hashCode = -1530764141;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(MethodName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(MethodClass);
            return hashCode;
        }

        public static bool operator ==(MethodInfo info1, MethodInfo info2)
        {
            return EqualityComparer<MethodInfo>.Default.Equals(info1, info2);
        }

        public static bool operator !=(MethodInfo info1, MethodInfo info2)
        {
            return !(info1 == info2);
        }
    }
}
