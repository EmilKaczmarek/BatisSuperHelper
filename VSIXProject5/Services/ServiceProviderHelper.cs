using EnvDTE80;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace iBatisSuperHelper.Services
{
    public class ServiceProviderHelper : IServiceProviderHelper
    {

        private readonly IVsTextManager _sVsTextManager;
        private readonly IComponentModel _componentModel;
        private readonly DTE2 _dte;

        public ServiceProviderHelper(IVsTextManager sVsTextManager, IComponentModel componentModel, DTE2 dte2)
        {
            _sVsTextManager = sVsTextManager;
            _componentModel = componentModel;
            _dte = dte2;
        }

        public IVsTextView GetActiveTextView()
        {
            _sVsTextManager.GetActiveView(1, null, out var textView);

            return textView;
        }

        public IWpfTextView GetActiveWpfTextView()
        {
            return _componentModel.GetService<IVsEditorAdaptersFactoryService>().GetWpfTextView(GetActiveTextView());
        }
    }
}