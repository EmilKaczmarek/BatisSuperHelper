using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.Actions.TextProviders
{
    public interface IDocumentPropertiesProvider
    {
        string GetContentType();
        string GetPath();
        int GetSelectionLineNumber();
        object GetDocumentRepresentation();
        Type DocumentRepresentationType();
        string GetSelectionLineContent();
    }
}
