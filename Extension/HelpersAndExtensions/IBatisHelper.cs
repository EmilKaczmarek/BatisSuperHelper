using System.Linq;
using IBatisSuperHelper.Constants.BatisConstants;

namespace IBatisSuperHelper.Helpers
{
    public static class IBatisHelper
    {
        public static bool IsIBatisStatment(string statment)
        {
            return XmlMapConstants.StatementNames.Any(e => e == statment);
        }
    }
}
