using BatisSuperHelper.VSIntegration.ErrorList;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.Validation.XmlValidators
{
    public interface IBufferValidator
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
        /// Trigger change
        /// </summary>
        /// <param name="span"></param>
        void OnChange(SnapshotSpan newSpan);

        /// <summary>
        /// Validate current spans.
        /// </summary>
        void ValidateAllSpans();

        /// <summary>
        /// Adds errors to ErrorList
        /// </summary>
        void AddToErrorList();

        string FilePath { get; }
    }
}
