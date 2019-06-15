using BatisSuperHelper.VSIntegration.ErrorList;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.Validation.XmlValidators
{
    public interface IXmlValidator
    {
        /// <summary>
        /// Determine if validator is running(scanning document/spans)
        /// </summary>
        bool IsRunning { get; }
        /// <summary>
        /// List of errors that validator returns
        /// </summary>
        List<BatisError> Errors { get; }
        /// <summary>
        /// Action when buffer change.
        /// </summary>
        /// <param name="span"></param>
        void OnChange(SnapshotSpan newSpan);

        string FilePath { get; }
    }
}
