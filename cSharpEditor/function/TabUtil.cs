using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using System.Linq;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using System.Reflection;

namespace cSharpEditor.function
{
    public class TabUtil
    {
        private static CompletionWindow _completionWindow;
        private static List<string> _xamlTagsCache = new List<string>();

        // エディタ起動時（MainWindowのコンストラクタ等）で一度だけ呼び出してくださいませ
        public static void InitializeXamlIntellisense()
        {
            if (_xamlTagsCache.Count > 0) return; // 既に取得済みならスキップします

            var wpfAssembly = typeof(System.Windows.FrameworkElement).Assembly;

            _xamlTagsCache = wpfAssembly.GetTypes()
                .Where(t => t.IsPublic && !t.IsAbstract && typeof(System.Windows.FrameworkElement).IsAssignableFrom(t))
                .Select(t => t.Name)
                .OrderBy(name => name)
                .ToList();
        }

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
        // キー入力された直後の処理
        // キー入力された直後の処理
        private static void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (sender is not TextArea textArea) return;

            int offset = textArea.Caret.Offset;
            var document = textArea.Document;

            // ※ タブのTagなどから現在のファイルがXAMLかどうかを判定するフラグです
            // 卿の環境に合わせて、適切に拡張子判定を行うよう書き換えてくださいませ
            bool isXamlFile = true;

            // ＝＝＝ XAMLファイル用の処理 ＝＝＝
            if (isXamlFile)
            {
                // ① タグの補完（「<」が入力された場合）
                if (e.Text == "<")
                {
                    if (_xamlTagsCache.Count == 0) InitializeXamlIntellisense();

                    if (_xamlTagsCache.Count > 0)
                    {
                        _completionWindow = new CompletionWindow(textArea);
                        var data = _completionWindow.CompletionList.CompletionData;
                        foreach (var tagName in _xamlTagsCache)
                        {
                            data.Add(new MyCompletionData(tagName));
                        }
                        _completionWindow.Show();
                        _completionWindow.Closed += delegate { _completionWindow = null; };
                    }
                    return;
                }
                // ② プロパティの補完（「.」が入力された場合：例 Grid. など）
                else if (e.Text == ".")
                {
                    // ドットの直前にある単語（クラス名）を抽出いたします
                    int startOffset = offset - 2; // ドットの前の文字から逆引きします
                    while (startOffset >= 0 && char.IsLetterOrDigit(document.GetCharAt(startOffset)))
                    {
                        startOffset--;
                    }
                    startOffset++;

                    string className = document.GetText(startOffset, offset - 1 - startOffset);

                    if (!string.IsNullOrEmpty(className))
                    {
                        var wpfAssembly = typeof(System.Windows.FrameworkElement).Assembly;
                        // 抽出した単語と一致するWPFのクラスを探し出します
                        var targetType = wpfAssembly.GetTypes().FirstOrDefault(t => t.Name == className);

                        if (targetType != null)
                        {
                            _completionWindow = new CompletionWindow(textArea);
                            var data = _completionWindow.CompletionList.CompletionData;

                            // そのクラスが持つ公開プロパティ（添付プロパティ含む）を抽出いたします
                            var properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                                                       .Select(p => p.Name)
                                                       .Distinct()
                                                       .OrderBy(n => n);

                            foreach (var prop in properties)
                            {
                                data.Add(new MyCompletionData(prop));
                            }

                            if (data.Count > 0)
                            {
                                _completionWindow.Show();
                                _completionWindow.Closed += delegate { _completionWindow = null; };
                            }
                        }
                    }
                    return;
                }
            }
            // ＝＝＝ C#ファイル用の処理（既存のRoslyn呼び出し） ＝＝＝
            else
            {
                bool shouldTriggerCSharp = false;

                if (e.Text == ".")
                {
                    shouldTriggerCSharp = true;
                }
                else if (e.Text == " ")
                {
                    if (offset >= 6)
                    {
                        string prevText = document.GetText(offset - 6, 6);
                        if (prevText == "using ")
                        {
                            shouldTriggerCSharp = true;
                        }
                    }
                }

                if (shouldTriggerCSharp)
                {
                    string code = document.Text;
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
