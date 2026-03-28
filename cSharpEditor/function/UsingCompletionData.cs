using System;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace cSharpEditor.function
{
    public class UsingCompletionData : ICompletionData
    {
        private readonly string _namespaceName;

        public UsingCompletionData(string namespaceName)
        {
            _namespaceName = namespaceName;
        }

        public ImageSource Image => null;
        public string Text => _namespaceName;
        public object Content => $"using {_namespaceName}; の追加";
        public object Description => $"不足しているディレクティブ {_namespaceName} を先頭に追加しますわ";
        public double Priority => 1;

        // 候補が選択された際の挿入処理です
        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            // カーソル位置の単語はそのまま残し、ドキュメントの最上段（0文字目）にusingを書き込みますのよ
            textArea.Document.Insert(0, $"using {_namespaceName};\r\n");
        }
    }
}