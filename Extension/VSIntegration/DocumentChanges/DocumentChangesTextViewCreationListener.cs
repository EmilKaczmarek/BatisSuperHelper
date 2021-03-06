﻿using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Reactive.Linq;
using BatisSuperHelper.HelpersAndExtensions.VisualStudio;
using BatisSuperHelper.VSIntegration.DocumentChanges.Actions;
using Microsoft.VisualStudio.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;

namespace BatisSuperHelper.VSIntegration.DocumentChanges
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("XML")]
    [ContentType("CSharp")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class DocumentChangesTextViewCreationListener : IWpfTextViewCreationListener
    {
        public void TextViewCreated(IWpfTextView textView)
        {
            if (textView?.Caret?.Position == null)
                return;

            var contentType = textView.Caret.Position.BufferPosition.Snapshot.GetContentTypeName();
            IOnFileContentChange fileAction = contentType == "CSharp" ? new  CSharpFileContentOnChange() : (IOnFileContentChange)new XmlFileContentOnChange();
            Observable.FromEventPattern<TextContentChangedEventArgs>(textView.TextBuffer, "Changed")
               //.Select(e => e.EventArgs.Changes)
               .DistinctUntilChanged()
               .Throttle(TimeSpan.FromMilliseconds(500))
               .Subscribe(e =>
               {
                   ThreadHelper.JoinableTaskFactory.RunAsync(async () => await fileAction.HandleChangeAsync(textView));
               });
        }
    }
}
