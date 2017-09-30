using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace iBatisSuperHelper.Services
{
    public interface IServiceProviderHelper
    {
        IVsTextView GetActiveTextView();
        IWpfTextView GetActiveWpfTextView();
    }
}