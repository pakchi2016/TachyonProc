using System;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace cSharpEditor.function
{
    // AvalonEditの補完ウィンドウに表示するアイテムの定義ですわ
    public class MyCompletionData : ICompletionData
    {
        public MyCompletionData(string text)
        {
            Text = text;
        }

        public ImageSource Image => null; // アイコン画像は一旦なしとします
        public string Text { get; }
        public object Content => Text; // リストに表示される文字列
        public object Description => "Roslynによる解析候補ですわ";
        public double Priority => 0;

        // ユーザーがEnterキー等で候補を確定させた際の挿入処理です
        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }
    }
}