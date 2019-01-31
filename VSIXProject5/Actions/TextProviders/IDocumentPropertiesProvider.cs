using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.Actions.TextProviders
{
    public interface IDocumentPropertiesProvider
    {
        string GetContentType();
        int GetSelectionLineNumber();
        object GetDocumentRepresentation();
        Type DocumentRepresentationType();
        string GetSelectionLineContent();
    }
}
