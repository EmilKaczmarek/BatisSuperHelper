using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.HelpersAndExtensions
{
    public static class IEnumerableIntExtensions
    {
        public static int DetermineClosestInt(this IEnumerable<int> elementsLineNumbers, int selectionLineNum)
        {
            var intList = elementsLineNumbers.ToList();
            int lineNumber = selectionLineNum;
            int? elementLocation = intList.Cast<int?>().FirstOrDefault(x => x == lineNumber);

            if (elementLocation == null)
            {
                intList.Add(lineNumber);
                intList.Sort();
                int indexOfLineNumber = intList.IndexOf(lineNumber);
                elementLocation = intList[indexOfLineNumber == 0 ? 0 : indexOfLineNumber - 1];
            }

            return elementLocation.Value;
        }
    }
}
