using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using System;

namespace BatisSuperHelper.Actions.TextProviders
{
    public class TextManagerPropertiesProvider : IDocumentPropertiesProvider
    {
        private IVsTextManager _textManager => _textManagerLazy.Value;
        private readonly Lazy<IVsTextManager> _textManagerLazy;

        private IVsEditorAdaptersFactoryService _editorAdapersFactory => _editorAdapersFactoryLazy.Value;
        private readonly Lazy<IVsEditorAdaptersFactoryService> _editorAdapersFactoryLazy;

        private IVsTextView _textView => _textViewLazy.Value;
        private readonly Lazy<IVsTextView> _textViewLazy;

        private SnapshotPoint _snapshotPoint => _snapshotPointLazy.Value;
        private readonly Lazy<SnapshotPoint> _snapshotPointLazy;

        public TextManagerPropertiesProvider(IVsTextManager textManager, IVsEditorAdaptersFactoryService editorAdaptersFactory)
        {
            _textManagerLazy = new Lazy<IVsTextManager>(() => textManager);
            _editorAdapersFactoryLazy = new Lazy<IVsEditorAdaptersFactoryService>(() => editorAdaptersFactory);

            _textViewLazy = new Lazy<IVsTextView>(GetActiveView);
            _snapshotPointLazy = new Lazy<SnapshotPoint>(GetSnaphotPoint);
        }

        private IVsTextView GetActiveView()
        {
            _textManager.GetActiveView(1, null, out IVsTextView textView);

            return textView;
        }

        private SnapshotPoint GetSnaphotPoint()
        { 
            return _editorAdapersFactory.GetWpfTextView(_textView).Caret.Position.BufferPosition;
        }

        public string GetPath()
        {
            _snapshotPoint.Snapshot.TextBuffer.Properties.TryGetProperty(typeof(IVsTextBuffer), out IVsTextBuffer bufferAdapter);

            if (bufferAdapter is IPersistFileFormat)
            {
                (bufferAdapter as IPersistFileFormat).GetCurFile(out string filePath, out var formatIndex);
                return filePath;
            }

            return null;
        }

        public string GetContentType()
        {
            _textManager.GetActiveView(1, null, out IVsTextView textView);
           
            return _snapshotPoint.Snapshot.ContentType.TypeName;
        }

        public int GetSelectionLineNumber()
        {
            _textView.GetCaretPos(out int selectionLineNum, out int selectionCol);

            return selectionLineNum;
        }

        public object GetDocumentRepresentation()
        {
            string content = GetContentType();
            if (content == "CSharp")
                return _snapshotPoint.Snapshot.GetOpenDocumentInCurrentContextWithChanges();

            if(content == "XML")
                return _snapshotPoint.Snapshot.GetText();

            return null;
        }

        public Type DocumentRepresentationType()
        {
            switch (GetContentType())
            {
                case "CSharp":
                    return typeof(Document);
                case "XML":
                    return typeof(string);
                default:
                    return typeof(object);
            }
        }

        public string GetSelectionLineContent()
        {
            return _snapshotPoint.GetContainingLine().GetText();
        }
    }
}
