using System.Linq;
using BatisSuperHelper.Constants.BatisConstants;

namespace BatisSuperHelper.Helpers
{
    public static class BatisConstantsHelper
    {
        public static bool IsBatisStatment(string statment)
        {
            return XmlMapConstants.StatementNames.Any(e => e == statment);
        }
    }
}
