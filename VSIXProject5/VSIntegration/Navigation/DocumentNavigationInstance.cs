using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.VSIntegration.Navigation
{
    public class DocumentNavigationInstance
    {
        public static DocumentNavigation instance;

        public static void InjectDTE(DTE2 dte2)
        {
            instance = new DocumentNavigation(dte2);
        }
    }
}
