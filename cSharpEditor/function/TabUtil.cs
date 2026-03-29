using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using System.Linq;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;

namespace cSharpEditor.function
{
    public class TabUtil
    {
        private static CompletionWindow _completionWindow;

        // 新しいタブとエディタを生成する処理
        public static void AddNewTab(string title,TabControl EditorTabControl)
        {
            // エディタコンポーネントの生成と設定
            var textEditor = new TextEditor
            {
                ShowLineNumbers = true, // サクラエディタ風の要である行番号表示
                FontFamily = new FontFamily("Consolas"), // プログラミング用等幅フォント
                FontSize = 14,
                // 初期状態からC#のシンタックスハイライトを適用いたします
                SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("C#")
            };

            textEditor.TextArea.KeyDown += TextArea_KeyDown;
            textEditor.TextArea.TextEntered += TextArea_TextEntered;
            textEditor.TextArea.TextEntering += TextArea_TextEntering;

            // タブの生成
            var tabItem = new TabItem
            {
                Header = title,
                Content = textEditor,
                Tag = null
            };

            // 画面のタブコントロールに追加し、表示を切り替える
            EditorTabControl.Items.Add(tabItem);
            EditorTabControl.SelectedItem = tabItem;
        }

        // 指定された内容で新しいタブを開く処理
        public static void OpenNewFileTab(string title, string content, string filePath,TabControl EditorTabControl)
        {
            var textEditor = new TextEditor
            {
                ShowLineNumbers = true,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 14,
                Text = content // 読み込んだファイルの内容をセットします
            };

            // 拡張子によってシンタックスハイライトを切り替えますわ
            string extension = Path.GetExtension(filePath).ToLower();
            if (extension == ".cs")
            {
                textEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("C#");
            }
            else if (extension == ".xaml" || extension == ".xml")
            {
                textEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("XML");
            }

            var tabItem = new TabItem
            {
                Header = title,
                Content = textEditor,
                Tag = filePath
            };

            textEditor.TextArea.KeyDown += TextArea_KeyDown;
            textEditor.TextArea.TextEntering += TextArea_TextEntering;
            textEditor.TextArea.TextEntered += TextArea_TextEntered;
            EditorTabControl.Items.Add(tabItem);
            EditorTabControl.SelectedItem = tabItem;
        }

        private static void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && _completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    _completionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }

        // キー入力された直後の処理（「.」や「using 」を検知してRoslynを呼び出します）
        private static void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (sender is not TextArea textArea) return;

            bool shouldTrigger = false;
            int offset = textArea.Caret.Offset;

            // ① ドットが入力された場合は無条件でトリガーとします
            if (e.Text == ".")
            {
                shouldTrigger = true;
            }
            // ② スペースが入力された場合は、直前の単語を確認します
            else if (e.Text == " ")
            {
                if (offset >= 6)
                {
                    string prevText = textArea.Document.GetText(offset - 6, 6);
                    if (prevText == "using ") // 直前が「using 」ならトリガーとしますわ
                    {
                        shouldTrigger = true;
                    }
                }
            }

            // トリガー条件を満たした場合のみ、解析とポップアップ表示を行います
            if (shouldTrigger)
            {
                string code = textArea.Document.Text;

                // Roslynの解析処理を呼び出します
                var completions = RoslynUtil.GetCompletions(code, offset);

                if (completions.Any())
                {
                    _completionWindow = new CompletionWindow(textArea);
                    var data = _completionWindow.CompletionList.CompletionData;

                    foreach (var c in completions)
                    {
                        data.Add(new MyCompletionData(c));
                    }

                    _completionWindow.Show();
                    _completionWindow.Closed += delegate { _completionWindow = null; };
                }
            }
        }
        private static void TextArea_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl + . が押されたかどうかを判定します
            if (e.Key == Key.OemPeriod && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (sender is not TextArea textArea) return;

                int offset = textArea.Caret.Offset;
                string code = textArea.Document.Text;

                // ステップ2で作った推論エンジンを呼び出します
                var missingUsings = RoslynUtil.GetMissingUsings(code, offset);

                if (missingUsings.Any())
                {
                    _completionWindow = new CompletionWindow(textArea);
                    var data = _completionWindow.CompletionList.CompletionData;

                    // 見つかった候補（Pathなら複数出ることもありますわ）をリストに登録します
                    foreach (var ns in missingUsings)
                    {
                        data.Add(new UsingCompletionData(ns));
                    }

                    _completionWindow.Show();
                    _completionWindow.Closed += delegate { _completionWindow = null; };

                    // ドットがエディタに入力されるのを防ぎます
                    e.Handled = true;
                }
            }
        }
    }
}
