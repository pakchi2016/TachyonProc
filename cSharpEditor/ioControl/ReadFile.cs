using cSharpEditor.function;
using ICSharpCode.AvalonEdit;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Controls;

namespace cSharpEditor.ioControl
{
    public class FileControl
    {
        public static void OpenCode(TabControl EditorTabControl)
        {
            var dialog = new OpenFileDialog
            {
                Title = "ファイルを開く",
                Filter = "C# ファイル (*.cs)|*.cs|XAML ファイル (*.xaml)|*.xaml|すべてのファイル (*.*)|*.*"
            };

            // ダイアログで「開く」が選択された場合
            if (dialog.ShowDialog() == true)
            {
                string filePath = dialog.FileName;
                string fileName = Path.GetFileName(filePath);
                string content = File.ReadAllText(filePath);

                TabUtil.OpenNewFileTab(fileName, content, filePath, EditorTabControl);
            }
        }
        public static void SaveCode(TabControl tabControl, bool isSaveAs = false)
        {
            // 現在選択されているタブとエディタを取得します
            if (tabControl.SelectedItem is not TabItem selectedTab ||
                selectedTab.Content is not TextEditor textEditor)
            {
                return;
            }

            string currentPath = selectedTab.Tag as string;

            // 「名前を付けて保存」または「まだ一度も保存されていない新規ファイル」の場合
            if (isSaveAs || string.IsNullOrEmpty(currentPath))
            {
                var dialog = new SaveFileDialog
                {
                    Title = "保存",
                    Filter = "C# ファイル (*.cs)|*.cs|XAML ファイル (*.xaml)|*.xaml|すべてのファイル (*.*)|*.*",
                    FileName = selectedTab.Header.ToString()
                };

                if (dialog.ShowDialog() == true)
                {
                    currentPath = dialog.FileName;
                }
                else
                {
                    return; // キャンセルされた場合は中断しますわ
                }
            }

            // ディスクへファイル内容を書き込みます
            File.WriteAllText(currentPath, textEditor.Text);

            // タブの表示名と記憶しているパスを最新状態に更新しますわ
            selectedTab.Tag = currentPath;
            selectedTab.Header = Path.GetFileName(currentPath);

            // 新規保存時のために、拡張子に応じたハイライト設定を再適用します
            string extension = Path.GetExtension(currentPath).ToLower();
            if (extension == ".cs")
            {
                textEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("C#");
            }
            else if (extension == ".xaml" || extension == ".xml")
            {
                textEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("XML");
            }
        }
    }
}
