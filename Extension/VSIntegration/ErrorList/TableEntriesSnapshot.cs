using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatisSuperHelper.VSIntegration.ErrorList
{
    public class TableEntriesSnapshot : TableEntriesSnapshotBase
    {
        public string FilePath { get; }
        public override int VersionNumber => 0;
        public override int Count => _errors.Count;

        private readonly List<BatisError> _errors = new List<BatisError>();//Should this be readonly/immutable?

        public TableEntriesSnapshot(string filePath, IEnumerable<BatisError> batisErrors)
        {
            FilePath = filePath;
            _errors.AddRange(batisErrors);
        }

        public override bool TryGetValue(int index, string keyName, out object content)
        {
            content = null;
            if ((index >= 0) && (index < _errors.Count))
            {
                content = GetValueByTableKeyName(index, keyName);
            }
            return content != null;
        }

        private object GetValueByTableKeyName(int index, string keyName)
        {
            switch (keyName)
            {
                case StandardTableKeyNames.BuildTool:
                    return _errors[index].BuildTools;
                case StandardTableKeyNames.Column:
                    return _errors[index].Column;
                case StandardTableKeyNames.DetailsExpander://What is this???
                    return null;
                case StandardTableKeyNames.DocumentName:
                    return _errors[index].Document;
                case StandardTableKeyNames.ErrorCategory://Not sure what this represents...
                    return _errors[index].ErrorCategory;
                case StandardTableKeyNames.ErrorCode:
                    return _errors[index].ErrorCode;
                case StandardTableKeyNames.ErrorCodeToolTip://Again, what is this?
                    return "Tooltip";
                case StandardTableKeyNames.ErrorRank://... 
                    return "Error";//TODO: this looks pretty important. Figure this out.
                case StandardTableKeyNames.ErrorSeverity:
                    return _errors[index].ErrorSeverity;
                case StandardTableKeyNames.ImageIndex:
                case StandardTableKeyNames.PriorityImage:
                    return null;
                case StandardTableKeyNames.IsActiveContext://Was the error generated from the active context?
                    return true;
                case StandardTableKeyNames.Line:
                    return _errors[index].Line;
                case StandardTableKeyNames.Priority:
                    return _errors[index].Priority;
                case StandardTableKeyNames.ProjectGuid://Ignore for now
                    return null;
                case StandardTableKeyNames.ProjectName:
                    return GetProjectNameSafe(_errors[index].Document);
                case StandardTableKeyNames.TaskCategory://How it differs from ErrorCategory???
                    return _errors[index].TaskCategory;
                case StandardTableKeyNames.Text:
                    return _errors[index].Text;

                //Possibly not needed cases.
                case StandardTableKeyNames.OutputWindowMessageProvider:
                case StandardTableKeyNames.OutputWindowMessageId:
                    return null;

            }
            return null;
        }

        private string GetProjectNameSafe(string filePath)
        {
            if (ThreadHelper.CheckAccess())
            {
                //This call is from UI Thread, so I should not throw.
                return GetProjectNameUnsafe(filePath);
            }

            //Switch to UI Thread and call GetProjectNameUnsafe
            return ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                return GetProjectNameUnsafe(filePath);
            });
        }

        /// <summary>
        /// Get ProjectName from FilePath, throws when not on UI Thread.
        /// </summary>
        /// <param name="filePath">File Path</param>
        /// <returns>Project name</returns>
        private string GetProjectNameUnsafe(string filePath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var projectItem = GotoAsyncPackage.EnvDTE?.Solution.FindProjectItem(filePath);
            return projectItem != null && projectItem.ContainingProject != null
                ? projectItem.ContainingProject.Name
                : null;
        }

    }
}
