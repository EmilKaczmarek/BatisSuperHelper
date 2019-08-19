using BatisSuperHelper.Actions.PrettyPrint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests.PrettyPrintTests
{
    public class PrettyPrintServiceTests
    {
        [Fact]
        public void ShouldRemoveCDataSectionsForStartAndEndSectionsInNewLine()
        {
            var sql = @"<![CDATA[
                            Select * from Person
                        ]]>";
            var service = new PrettyPrintService();
            var result = service.PrettyPrint(sql);
        }
    }
}
