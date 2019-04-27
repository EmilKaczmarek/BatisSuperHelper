using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.EventHandlers.SolutionEventsActions
{
    public interface IVSSolutionEventsActions
    {
        void OnSolutionLoadComplete();
        void SolutionOnClose();
    }
}
